using System;
using Xamarin.Forms;

namespace Axemasta.SuperWebView
{
    public class SuperWebNavigatedEventArgs : SuperWebNavigationEventArgs
    {
        public SuperWebNavigatedEventArgs(WebNavigationEvent navigationEvent, SuperWebViewSource source, string url, WebNavigationResult result)
            : base(navigationEvent, source, url)
        {
            Result = result;
        }

        public WebNavigationResult Result { get; private set; }
    }
}
