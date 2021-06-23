using System;
using System.Threading.Tasks;
using CoreFoundation;
using Foundation;
using WebKit;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Axemasta.SuperWebView.iOS
{
    public class SuperWebViewNavigationDelegate : WKNavigationDelegate
    {
		private NSUrl _lastWebsite;

		private bool _pageIsLocal;

		readonly SuperWkWebViewRenderer _renderer;
		WebNavigationEvent _lastEvent;

		public SuperWebViewNavigationDelegate(SuperWkWebViewRenderer renderer)
		{
			if (renderer is null)
				throw new ArgumentNullException(nameof(renderer));

			_renderer = renderer;

			_pageIsLocal = false;
		}

		SuperWebView WebView => _renderer.WebView;

		string GetCurrentUrl()
		{
			return _renderer?.Url?.AbsoluteUrl?.ToString();
		}

		public override void DidFailNavigation(WKWebView webView, WKNavigation navigation, NSError error)
		{
			var url = GetCurrentUrl();
			WebView.SendNavigated(
				new SuperWebNavigatedEventArgs(_lastEvent, new SuperUrlWebViewSource { Url = url }, url, WebNavigationResult.Failure)
			);

			_renderer.UpdateCanGoBackForward();
		}

		public override void DidFailProvisionalNavigation(WKWebView webView, WKNavigation navigation, NSError error)
		{
			var url = GetCurrentUrl();
			WebView.SendNavigated(
				new SuperWebNavigatedEventArgs(_lastEvent, new SuperUrlWebViewSource { Url = url }, url, WebNavigationResult.Failure)
			);

			_renderer.UpdateCanGoBackForward();
		}

		public bool IsPageLocal()
        {
			return _pageIsLocal;
        }

		public NSUrl GetLastWebsite()
        {
			return _lastWebsite;
        }

		public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
		{
			if (webView.IsLoading)
				return;

			var url = GetCurrentUrl();
			if (url == $"file://{NSBundle.MainBundle.BundlePath}/")
            {
				ProcessNavigatedToLocal(url);
				return;
            }

			_lastWebsite = _renderer?.Url;

			_renderer._ignoreSourceChanges = true;
			WebView.SetValueFromRenderer(SuperWebView.SourceProperty, new SuperUrlWebViewSource { Url = url });
			_renderer._ignoreSourceChanges = false;

			ProcessNavigatedToWebsite(url);
		}

		public void ProcessNavigatedToLocal(string url)
		{
			_pageIsLocal = true;
			var args = new SuperWebNavigatedEventArgs(_lastEvent, WebView.Source, url, WebNavigationResult.Success);
			WebView.SendNavigated(args);
			_renderer.UpdateCanGoBackForward();
		}

		async void ProcessNavigatedToWebsite(string url)
		{
			_pageIsLocal = false;
			try
			{
				if (_renderer?.WebView?.Cookies != null)
					await _renderer.SyncNativeCookiesToElement(url);
			}
			catch (Exception exc)
			{
				Log.Warning(nameof(SuperWkWebViewRenderer), $"Failed to Sync Cookies {exc}");
			}

			var args = new SuperWebNavigatedEventArgs(_lastEvent, WebView.Source, url, WebNavigationResult.Success);
			WebView.SendNavigated(args);
			_renderer.UpdateCanGoBackForward();
		}

		public override void DidStartProvisionalNavigation(WKWebView webView, WKNavigation navigation)
		{
		}

		// https://stackoverflow.com/questions/37509990/migrating-from-uiwebview-to-wkwebview
		public override void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
		{
			var navEvent = WebNavigationEvent.NewPage;
			var navigationType = navigationAction.NavigationType;
			switch (navigationType)
			{
				case WKNavigationType.LinkActivated:
					navEvent = WebNavigationEvent.NewPage;

					if (navigationAction.TargetFrame == null)
						webView?.LoadRequest(navigationAction.Request);

					break;
				case WKNavigationType.FormSubmitted:
					navEvent = WebNavigationEvent.NewPage;
					break;
				case WKNavigationType.BackForward:
					navEvent = _renderer._lastBackForwardEvent;
					break;
				case WKNavigationType.Reload:
					navEvent = WebNavigationEvent.Refresh;
					break;
				case WKNavigationType.FormResubmitted:
					navEvent = WebNavigationEvent.NewPage;
					break;
				case WKNavigationType.Other:
					navEvent = WebNavigationEvent.NewPage;
					break;
			}

			_lastEvent = navEvent;
			var request = navigationAction.Request;
			var lastUrl = request.Url.ToString();

			/* Without this the deferral token will cause a completely undiagnosable termination **/

			if (lastUrl.StartsWith("file://"))
            {
				var backArgs = new SuperWebNavigatingEventArgs(navEvent, new SuperUrlWebViewSource { Url = lastUrl }, lastUrl, false);

				WebView.SendNavigating(backArgs);
				_renderer.UpdateCanGoBackForward();

				decisionHandler(WKNavigationActionPolicy.Allow);
				return;
			}

			if (SiteAlreadyVisited(_renderer.BackForwardList, lastUrl))
            {
				var backArgs = new SuperWebNavigatingEventArgs(navEvent, new SuperUrlWebViewSource { Url = lastUrl }, lastUrl, false);

				WebView.SendNavigating(backArgs);
				_renderer.UpdateCanGoBackForward();

				decisionHandler(WKNavigationActionPolicy.Allow);
				return;
			}

            var args = new SuperWebNavigatingEventArgs(navEvent, new SuperUrlWebViewSource { Url = lastUrl }, lastUrl, true);

			/*
			 * Register the deferral before sending the args incase the code using the deferral token
			 * is not executed async. In that scenario the token completion will be called before the
			 * callback is registered and the decision handler won't get called
			 */
			args.RegisterDeferralCompletedCallBack(() => NavigatingDeterminedCallback(args, decisionHandler));

			WebView.SendNavigating(args);
			_renderer.UpdateCanGoBackForward();

			// user is not trying to cancel navigation, allow navigation
			if (!args.DeferralRequested)
			{
				decisionHandler(WKNavigationActionPolicy.Allow);
				return;
			}
		}

		bool UrlMatches(NSUrl url1, NSUrl url2)
        {
			if (url1 == null || url2 == null)
				return false;

			if (url1 == url2)
				return true;

			if (url1.AbsoluteString.Contains(url2.AbsoluteString))
				return true;

			if (url2.AbsoluteString.Contains(url1.AbsoluteString))
				return true;

			return false;
        }

		bool SiteAlreadyVisited(WKBackForwardList wkBackForwardList, string url)
        {
			if (wkBackForwardList is null) return false;
			if (wkBackForwardList.BackItem is null && wkBackForwardList.CurrentItem is null) return false;

			try
            {
				var nsUrl = new NSUrl(url);

				if (UrlMatches(wkBackForwardList.CurrentItem.Url, nsUrl))
					return true;

				var backItems = wkBackForwardList.BackList;

                foreach (var backItem in backItems)
                {
					if (UrlMatches(backItem.Url, nsUrl))
						return true;
                }
            }
			catch (Exception ex)
            {
				Log.Warning(nameof(SuperWebViewNavigationDelegate), "An exception occurred determining whether site had already been visited");
				Log.Warning(nameof(SuperWebViewNavigationDelegate), ex.ToString());
			}

			return false;
		}

		async Task NavigatingDeterminedCallback(SuperWebNavigatingEventArgs args, Action<WKNavigationActionPolicy> decisionHandler)
		{
			/* 
			 * Decision handler MUST be called otherwise WKWebView throws the following exception:
			 * Objective-C exception thrown.  Name: NSInternalInconsistencyException Reason: 
			 * Completion handler passed to -[Xamarin_Forms_Platform_iOS_WkWebViewRenderer_CustomWebViewNavigationDelegate webView:decidePolicyForNavigationAction:decisionHandler:] was not called
			 */

			/*
			 * The previous implementation was causing a crash when the UI invoked a url change by performing
			 * an action like pressing a Button and updating the SuperWebView source.
			 * 
			 * The crash had very limited diagnostics but here is a diagnostic from the simulator:
			 * 10:06:35.541054+0100	com.apple.CoreSimulator.SimDevice.00000000-0000-0000-0000-000000000000
			 * (UIKitApplication:com.axemasta.SuperWebViewSample[f025][rb-legacy][86754]) 
			 * Service exited due to SIGTRAP
			 * 
			 * I found the following xamarin-macios posts:
			 * https://github.com/xamarin/xamarin-macios/issues/4130
			 * https://github.com/xamarin/xamarin-macios/pull/4312/files
			 * 
			 * It looks like when blocking a thread, it must be released back on the main thread otherwise the WKWebView
			 * will crash. I have updated the implementation to directly access the iOS threads because Device.BeginInvoke...
			 * was still causing the issue to occur
			 */

			Action action = () => DetermineNavigating(args, decisionHandler);

			if (NSThread.IsMain)
            {
				action.Invoke();
				return;
            }

			DispatchQueue.MainQueue.DispatchSync(action);
		}

		void DetermineNavigating(SuperWebNavigatingEventArgs args, Action<WKNavigationActionPolicy> decisionHandler)
		{
			
			var action = args.Cancelled ? WKNavigationActionPolicy.Cancel : WKNavigationActionPolicy.Allow;

			if (action == WKNavigationActionPolicy.Cancel)
				WebView.SendNavigationCancelled(new NavigationCancelledEventArgs(args.Url));

			decisionHandler(action);
		}
    }
}
