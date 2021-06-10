using WebKit;

namespace Axemasta.SuperWebView.iOS
{
    /// <summary>
    /// WKWebView Helper
    /// </summary>
    public static class WKWebViewHelper
    {
        internal static WKProcessPool _sharedPool;

        /// <summary>
        /// Create Pooled Configuration
        /// </summary>
        /// <returns></returns>
        public static WKWebViewConfiguration CreatedPooledConfiguration()
        {
            /*
             * https://developer.apple.com/forums/thread/99674
             * WKWebView and making sure cookies synchronize is really quirky
             * The main workaround I've found for ensuring that cookies synchronize 
             * is to share the Process Pool between all WkWebView instances.
             * It also has to be shared at the point you call init
             */

            var config = new WKWebViewConfiguration();

            if (_sharedPool == null)
            {
                _sharedPool = config.ProcessPool;
            }
            else
            {
                config.ProcessPool = _sharedPool;
            }

            return config;
        }

        public static void DisposeSharedPool()
        {
            _sharedPool.Dispose();
            _sharedPool = null;
        }
    }
}
