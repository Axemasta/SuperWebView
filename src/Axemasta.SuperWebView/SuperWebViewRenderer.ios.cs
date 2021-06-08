using System;
using System.Drawing;
using Axemasta.SuperWebView;
using Axemasta.SuperWebView.Internals;
using Foundation;
using WebKit;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.iOS;
using PreserveAttribute = Foundation.PreserveAttribute;

[assembly: ExportRenderer(typeof(SuperWebView), typeof(SuperWebViewRenderer))]
namespace Axemasta.SuperWebView
{
    public class Benched_SuperWebViewRenderer : WKWebView
    {
		static WKProcessPool _sharedPool;

		[Preserve(Conditional = true)]
		public Benched_SuperWebViewRenderer() : this(CreateConfiguration())
		{

		}

		[Preserve(Conditional = true)]
		public Benched_SuperWebViewRenderer(WKWebViewConfiguration config)
			: base(RectangleF.Empty, config)
		{

		}

		// https://developer.apple.com/forums/thread/99674
		// WKWebView and making sure cookies synchronize is really quirky
		// The main workaround I've found for ensuring that cookies synchronize 
		// is to share the Process Pool between all WkWebView instances.
		// It also has to be shared at the point you call init
		static WKWebViewConfiguration CreateConfiguration()
		{
			var config = new WKWebViewConfiguration();
			if (_sharedPool == null)
			{
				_sharedPool = config.ProcessPool;
			}
			else
			{
				config.ProcessPool = _sharedPool;
			}
			return config;
		}
	}

	public class SuperWebViewRenderer : ViewRenderer<SuperWebView, WKWebView>
	{
		private WKWebView _wkWebView;

		private readonly WKUserContentController _userController;
		private readonly WKWebViewConfiguration _webViewConfig;

		public SuperWebViewRenderer()
			: this(new WKWebViewConfiguration())
		{
		}

		public SuperWebViewRenderer(WKWebViewConfiguration config)
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
