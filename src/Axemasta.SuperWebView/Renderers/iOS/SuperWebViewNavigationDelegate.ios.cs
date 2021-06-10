using System;
using System.Threading.Tasks;
using Foundation;
using WebKit;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Axemasta.SuperWebView.iOS
{
    public class SuperWebViewNavigationDelegate : WKNavigationDelegate
    {
		readonly SuperWkWebViewRenderer _renderer;
		WebNavigationEvent _lastEvent;

		public SuperWebViewNavigationDelegate(SuperWkWebViewRenderer renderer)
		{
			if (renderer is null)
				throw new ArgumentNullException(nameof(renderer));

			_renderer = renderer;
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

		public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
		{
			if (webView.IsLoading)
				return;

			var url = GetCurrentUrl();
			if (url == $"file://{NSBundle.MainBundle.BundlePath}/")
				return;

			_renderer._ignoreSourceChanges = true;
			WebView.SetValueFromRenderer(SuperWebView.SourceProperty, new SuperUrlWebViewSource { Url = url });
			_renderer._ignoreSourceChanges = false;

			ProcessNavigated(url);
		}

		async void ProcessNavigated(string url)
		{
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
			}
		}

		async Task NavigatingDeterminedCallback(SuperWebNavigatingEventArgs args, Action<WKNavigationActionPolicy> decisionHandler)
		{
			/* 
			 * Decision handler MUST be called otherwise WKWebView throws the following exception:
			 * Objective-C exception thrown.  Name: NSInternalInconsistencyException Reason: 
			 * Completion handler passed to -[Xamarin_Forms_Platform_iOS_WkWebViewRenderer_CustomWebViewNavigationDelegate webView:decidePolicyForNavigationAction:decisionHandler:] was not called
			 */

			Func<Task> navigationTask = () => Task.Run(() => DetermineNavigating(args, decisionHandler));

			if (Device.IsInvokeRequired)
				await Device.InvokeOnMainThreadAsync(navigationTask);
			else
				await navigationTask();
		}

		void DetermineNavigating(SuperWebNavigatingEventArgs args, Action<WKNavigationActionPolicy> decisionHandler)
		{
			var cancel = args.Cancelled ? WKNavigationActionPolicy.Cancel : WKNavigationActionPolicy.Allow;

			if (cancel == WKNavigationActionPolicy.Cancel)
				Console.WriteLine("Navigation is being cancelled");

			decisionHandler(cancel);
		}
	}
}
