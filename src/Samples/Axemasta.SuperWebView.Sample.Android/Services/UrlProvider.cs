using Axemasta.SuperWebView.Sample.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(Axemasta.SuperWebView.Sample.Droid.Services.UrlProvider))]

namespace Axemasta.SuperWebView.Sample.Droid.Services
{
    public class UrlProvider : IUrlProvider
    {
        public string GetBaseUrl()
        {
            return "file:///android_asset/";
        }
    }
}
