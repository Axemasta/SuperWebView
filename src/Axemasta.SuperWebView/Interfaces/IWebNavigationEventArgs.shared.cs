using System;
using Xamarin.Forms;

namespace Axemasta.SuperWebView
{
	public interface ISuperWebNavigationEventArgs
	{
		WebNavigationEvent NavigationEvent { get; }

		SuperWebViewSource Source { get; }

		string Url { get; }
	}
}
