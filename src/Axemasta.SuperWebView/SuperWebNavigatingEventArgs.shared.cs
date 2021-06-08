using System;
using Xamarin.Forms;

namespace Axemasta.SuperWebView
{
    public class SuperWebNavigatingEventArgs : SuperWebNavigationEventArgs
    {
        public SuperWebNavigatingEventArgs(WebNavigationEvent navigationEvent, WebViewSource source, string url)
            : base(navigationEvent, source, url)
        {
        }

        public bool Cancel { get; set; }
    }
}
