using System;
using Xamarin.Forms;

namespace Axemasta.SuperWebView
{
    public class SuperWebNavigatingEventArgs : DeferrableEventArgs, ISuperWebNavigationEventArgs
    {
        public WebNavigationEvent NavigationEvent { get; set; }

        public SuperWebViewSource Source { get; set; }

        public string Url { get; set; }
        
        public SuperWebNavigatingEventArgs(WebNavigationEvent navigationEvent, SuperWebViewSource source, string url, bool canCancel)
            : base(canCancel)
        {
            NavigationEvent = navigationEvent;
            Source = source;
            Url = url;
        }   
    }
}
