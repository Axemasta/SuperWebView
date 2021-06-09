using Axemasta.SuperWebView;
using Axemasta.SuperWebView.Internals;
using Xamarin.Forms;

#if __IOS__
using Xamarin.Forms.Platform.iOS;
[assembly: ExportRenderer(typeof(SuperWebView), typeof(SuperWebViewLegacyRenderer))]
#endif
