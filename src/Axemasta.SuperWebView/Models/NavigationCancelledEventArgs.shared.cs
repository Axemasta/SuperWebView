using System;

namespace Axemasta.SuperWebView
{
    public class NavigationCancelledEventArgs : EventArgs
    {
        public string Url { get; }

        public NavigationCancelledEventArgs(string url)
        {
            Url = url;
        }
    }
}
