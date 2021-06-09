using System;
using Xamarin.Forms;

namespace Axemasta.SuperWebView
{
    public class SuperWebNavigationEventArgs
    {
		protected SuperWebNavigationEventArgs(WebNavigationEvent navigationEvent, WebViewSource source, string url)
		{
			NavigationEvent = navigationEvent;
			Source = source;
			Url = url;
		}

		public WebNavigationEvent NavigationEvent { get; internal set; }

		public WebViewSource Source { get; internal set; }

		public string Url { get; internal set; }
	}
}
