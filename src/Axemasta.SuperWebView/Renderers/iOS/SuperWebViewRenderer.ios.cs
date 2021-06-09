using System;
using System.Drawing;
using Axemasta.SuperWebView;
using Axemasta.SuperWebView.Internals;
using Foundation;
using UIKit;
using WebKit;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.iOS;
using PreserveAttribute = Foundation.PreserveAttribute;

namespace Axemasta.SuperWebView.iOS
{
    public class Benched_SuperWebViewRenderer : WKWebView, IVisualElementRenderer, IWebViewDelegate, IEffectControlProvider, ITabStop
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

        public VisualElement Element => throw new NotImplementedException();

        public UIView NativeView => throw new NotImplementedException();

        public UIViewController ViewController => throw new NotImplementedException();

        public UIView TabStop => throw new NotImplementedException();

        public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

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
    }
}
