using System;
using Android.Webkit;
using Java.Interop;
using Xamarin.Forms.Internals;

namespace Axemasta.SuperWebView.Droid
{
    public class JSBridge : Java.Lang.Object
    {
        private readonly WeakReference<SuperWebViewRenderer> _renderer;

        public JSBridge(SuperWebViewRenderer renderer)
        {
            _renderer = new WeakReference<SuperWebViewRenderer>(renderer);
        }

        [JavascriptInterface]
        [Export("invokeAction")]
        public void InvokeAction(string data)
        {
            if (_renderer is null)
                return;

            if (!_renderer.TryGetTarget(out SuperWebViewRenderer renderer))
            {
                Log.Warning(nameof(JSBridge), "Could not get target for renderer");
                return;
            }

            if (renderer.SuperWebView is null)
            {
                Log.Warning(nameof(JSBridge), "Renderer Element was null");
                return;
            }

            renderer.SuperWebView.SendBrowserInvocation(new BrowserInvocationEventArgs(data));
        }
    }
}
