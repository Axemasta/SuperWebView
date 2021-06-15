using System;
namespace Axemasta.SuperWebView
{
    public class UrlEventArgs : EventArgs
    {
        public string NewUrl { get; }

        public UrlEventArgs(string newUrl)
        {
            this.NewUrl = newUrl;
        }
    }
}
