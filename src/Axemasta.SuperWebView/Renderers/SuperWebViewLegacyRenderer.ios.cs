using System;
using Axemasta.SuperWebView;
using Foundation;
using WebKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace Axemasta.SuperWebView
{
	public class SuperWebViewLegacyRenderer : ViewRenderer<SuperWebView, WKWebView>
	{
		private WKWebView _wkWebView;

		private readonly WKUserContentController _userController;
		private readonly WKWebViewConfiguration _webViewConfig;

		public SuperWebViewLegacyRenderer()
			: this(new WKWebViewConfiguration())
		{
		}

		public SuperWebViewLegacyRenderer(WKWebViewConfiguration config)
		{
			_webViewConfig = config;
			_userController = _webViewConfig.UserContentController;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<SuperWebView> e)
		{
			base.OnElementChanged(e);

			if (Control is null)
			{
				_wkWebView = new WKWebView(Frame, _webViewConfig)
				{
					NavigationDelegate = null
				};

				SetNativeControl(_wkWebView);
			}

			if (e.NewElement != null)
			{
				// Load Request

				if (Element.Source is SuperUrlWebViewSource urlSource)
				{
					var url = new NSUrl(urlSource.Url);

					var request = new NSUrlRequest(url);

					Control.LoadRequest(request);
				}
			}
		}
	}
}
