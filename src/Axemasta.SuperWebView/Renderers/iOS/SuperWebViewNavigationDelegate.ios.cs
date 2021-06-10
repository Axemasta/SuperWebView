using System;
using Foundation;
using WebKit;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.iOS;

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
			var args = new SuperWebNavigatingEventArgs(navEvent, new SuperUrlWebViewSource { Url = lastUrl }, lastUrl);

			WebView.SendNavigating(args);
			_renderer.UpdateCanGoBackForward();
			decisionHandler(args.Cancel ? WKNavigationActionPolicy.Cancel : WKNavigationActionPolicy.Allow);
		}

		string GetCurrentUrl()
		{
			return _renderer?.Url?.AbsoluteUrl?.ToString();
		}
	}
}
