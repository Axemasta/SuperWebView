using System;
using Axemasta.SuperWebView.Internals;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Axemasta.SuperWebView
{
    /// <summary>
    /// Super Web View Cross Platform Control
    /// </summary>
    public class LegacySuperWebView : WebView
    {
        // Placeholder until I get renderer working
    }

    [RenderWith(typeof(_SuperWebViewRenderer))]
    public class SuperWebView : View, ISuperWebViewController, IElementConfiguration<SuperWebView>
    {
        readonly Lazy<PlatformConfigurationRegistry<SuperWebView>> _platformConfigurationRegistry;

        public bool CanGoBack { get; set; }
        public bool CanGoForward { get; set; }

        public event EventHandler<EvalRequested> EvalRequested;
        public event EvaluateJavaScriptDelegate EvaluateJavaScriptRequested;
        public event EventHandler GoBackRequested;
        public event EventHandler GoForwardRequested;
        public event EventHandler ReloadRequested;

        public SuperWebView()
        {
            _platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<SuperWebView>>(() => new PlatformConfigurationRegistry<SuperWebView>(this));
        }

        public IPlatformElementConfiguration<T, SuperWebView> On<T>() where T : IConfigPlatform
        {
            return _platformConfigurationRegistry.Value.On<T>();
        }

        public void SendNavigated(SuperWebNavigatedEventArgs args)
        {
            throw new NotImplementedException();
        }

        public void SendNavigating(SuperWebNavigatingEventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
