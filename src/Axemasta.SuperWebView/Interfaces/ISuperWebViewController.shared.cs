using System;
using Xamarin.Forms.Internals;

namespace Axemasta.SuperWebView
{
    public interface ISuperWebViewController
    {
		bool CanGoBack { get; set; }
		bool CanGoForward { get; set; }
		event EventHandler<EvalRequested> EvalRequested;
		event EvaluateJavaScriptDelegate EvaluateJavaScriptRequested;
		event EventHandler GoBackRequested;
		event EventHandler GoForwardRequested;
		event EventHandler ReloadRequested;
		void SendNavigated(SuperWebNavigatedEventArgs args);
		void SendNavigating(SuperWebNavigatingEventArgs args);
	}
}
