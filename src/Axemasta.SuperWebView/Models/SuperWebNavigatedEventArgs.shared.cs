using Xamarin.Forms;

namespace Axemasta.SuperWebView
{
    public class SuperWebNavigatedEventArgs : ISuperWebNavigationEventArgs
    {
        public WebNavigationResult Result { get; }

        public WebNavigationEvent NavigationEvent { get; }

        public SuperWebViewSource Source { get; }

        public string Url { get; }

        public SuperWebNavigatedEventArgs(WebNavigationEvent navigationEvent, SuperWebViewSource source, string url, WebNavigationResult result)
        {
            NavigationEvent = navigationEvent;
            Source = source;
            Url = url;
            Result = result;
        }
    }
}
