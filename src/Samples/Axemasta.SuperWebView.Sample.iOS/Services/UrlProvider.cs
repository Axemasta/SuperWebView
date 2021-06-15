using Axemasta.SuperWebView.Sample.Services;
using Foundation;
using Xamarin.Forms;

[assembly: Dependency(typeof(Axemasta.SuperWebView.Sample.iOS.Services.UrlProvider))]

namespace Axemasta.SuperWebView.Sample.iOS.Services
{
    public class UrlProvider : IUrlProvider
    {
        public string GetBaseUrl()
        {
            return NSBundle.MainBundle.BundlePath;
        }
    }
}
