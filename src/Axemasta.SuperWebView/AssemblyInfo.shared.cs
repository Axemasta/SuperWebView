using Axemasta.SuperWebView;
using Axemasta.SuperWebView.Internals;
using Xamarin.Forms;

#if __IOS__
using Xamarin.Forms.Platform.iOS;
//[assembly: ExportRenderer(typeof(SuperWebView), typeof(Axemasta.SuperWebView.iOS.SuperWebViewLegacyRenderer))]
[assembly: ExportRenderer(typeof(SuperWebView), typeof(Axemasta.SuperWebView.iOS.SuperWkWebViewRenderer))]
#endif

#if __ANDROID__
using Xamarin.Forms.Platform.Android;
[assembly: ExportRenderer(typeof(SuperWebView), typeof(Axemasta.SuperWebView.Droid.SuperWebViewRenderer))]
#endif