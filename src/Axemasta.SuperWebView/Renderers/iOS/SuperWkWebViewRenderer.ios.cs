using System;
using System.Threading.Tasks;
using Axemasta.SuperWebView;
using Axemasta.SuperWebView.Internals;
using Foundation;
using UIKit;
using WebKit;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.iOS;
using PreserveAttribute = Foundation.PreserveAttribute;
using RectangleF = System.Drawing.RectangleF;

namespace Axemasta.SuperWebView.iOS
{
    public class SuperWkWebViewRenderer : WKWebView, IVisualElementRenderer, IWebViewDelegate, IEffectControlProvider, ITabStop
	{
        //TODO: Fix violation of encapsulation
        public bool _ignoreSourceChanges;
        public WebNavigationEvent _lastBackForwardEvent;

		[Preserve(Conditional = true)]
		public SuperWkWebViewRenderer()
            : this(WKWebViewHelper.CreatedPooledConfiguration())
		{

		}

		[Preserve(Conditional = true)]
		public SuperWkWebViewRenderer(WKWebViewConfiguration config)
			: base(RectangleF.Empty, config)
		{

		}

        public WebView WebView => Element as WebView;

        public VisualElement Element { get; private set; }

        public UIView NativeView => throw new NotImplementedException();

        public UIViewController ViewController => throw new NotImplementedException();

        public UIView TabStop => throw new NotImplementedException();

        public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

        public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
        {
            throw new NotImplementedException();
        }

        public void LoadHtml(string html, string baseUrl)
        {
            throw new NotImplementedException();
        }

        public void LoadUrl(string url)
        {
            throw new NotImplementedException();
        }

        public void RegisterEffect(Effect effect)
        {
            throw new NotImplementedException();
        }

        public void SetElement(VisualElement element)
        {
            throw new NotImplementedException();
        }

        public void SetElementSize(Xamarin.Forms.Size size)
        {
            throw new NotImplementedException();
        }

        public void UpdateCanGoBackForward()
        {
            ((IWebViewController)WebView).CanGoBack = CanGoBack;
            ((IWebViewController)WebView).CanGoForward = CanGoForward;
        }

        public async Task SyncNativeCookies(string url)
        {

        }

        public async Task SyncNativeCookiesToElement(string url)
        {

        }
    }
}
