using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using WebKit;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.iOS;
using PreserveAttribute = Foundation.PreserveAttribute;
using RectangleF = System.Drawing.RectangleF;

namespace Axemasta.SuperWebView.iOS
{
    public class SuperWkWebViewRenderer : WKWebView, IVisualElementRenderer, IWebViewDelegate, IEffectControlProvider, ITabStop
	{
        public SuperWebView WebView => Element as SuperWebView;

        public VisualElement Element { get; private set; }

        public UIView NativeView
        {
            get { return this; }
        }

        public UIViewController ViewController
        {
            get { return null; }
        }

        UIView ITabStop.TabStop => this;

        private readonly List<IDisposable> _disposables;

        public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

        protected virtual void OnElementChanged(VisualElementChangedEventArgs e) =>
            ElementChanged?.Invoke(this, e);

        //TODO: Fix violation of encapsulation
        public bool _ignoreSourceChanges;
        public WebNavigationEvent _lastBackForwardEvent;

        private const string EstimatedProgressKey = "estimatedProgress";

        bool _disposed;
        static int _sharedPoolCount = 0;
        static bool _firstLoadFinished = false;
        string _pendingUrl;
        EventTracker _events;

        VisualElementPackager _packager;
//#pragma warning disable CS0414
        VisualElementTracker _tracker;
//#pragma warning restore CS0414

        [Preserve(Conditional = true)]
		public SuperWkWebViewRenderer()
            : this(WKWebViewHelper.CreatedPooledConfiguration())
		{

		}

		[Preserve(Conditional = true)]
		public SuperWkWebViewRenderer(WKWebViewConfiguration config)
			: base(RectangleF.Empty, config)
		{
            _disposables = new List<IDisposable>();
		}

        #region Methods

        #region - Interface Methods

        public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
        {
            return NativeView.GetSizeRequest(widthConstraint, heightConstraint, 44, 44);
        }

        public void SetElement(VisualElement element)
        {
            var oldElement = Element;

            if (oldElement != null)
            {
                oldElement.PropertyChanged -= HandlePropertyChanged;
            }

            if (element != null)
            {
                Element = element;
                Element.PropertyChanged += HandlePropertyChanged;

                if (_packager == null)
                {
                    WebView.EvalRequested += OnEvalRequested;
                    WebView.EvaluateJavaScriptRequested += OnEvaluateJavaScriptRequested;
                    WebView.GoBackRequested += OnGoBackRequested;
                    WebView.GoForwardRequested += OnGoForwardRequested;
                    WebView.ReloadRequested += OnReloadRequested;
                    NavigationDelegate = new SuperWebViewNavigationDelegate(this);
                    UIDelegate = new SuperWebViewUIDelegate();

                    BackgroundColor = UIColor.Clear;

                    AutosizesSubviews = true;

                    var progress = AddObserver(EstimatedProgressKey, NSKeyValueObservingOptions.New, OnProgressUpdated);
                    _disposables.Add(progress);

                    _tracker = new VisualElementTracker(this);

                    _packager = new VisualElementPackager(this);
                    _packager.Load();

                    _events = new EventTracker(this);
                    _events.LoadEvents(this);
                }

                Load();
            }

            OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));

            EffectUtilities.RegisterEffectControlProvider(this, oldElement, element);

            if (Element != null && !string.IsNullOrEmpty(Element.AutomationId))
                AccessibilityIdentifier = Element.AutomationId;
        }

        /// <summary>
        /// Called When Progress Observer Updates
        /// </summary>
        /// <param name="nsObservedChange"></param>
        private void OnProgressUpdated(NSObservedChange nsObservedChange)
        {
            // Estimated Progress ranged from 0 => 1

            WebView.SendProgressChanged(new ProgressEventArgs(EstimatedProgress, 1));
        }

        public void SetElementSize(Size size)
        {
            Layout.LayoutChildIntoBoundingRegion(Element, new Rectangle(Element.X, Element.Y, size.Width, size.Height));
        }

        public void LoadHtml(string html, string baseUrl)
        {
            if (html is null)
                return;

            var url = baseUrl == null ? new NSUrl(NSBundle.MainBundle.BundlePath, true) : new NSUrl(baseUrl, true);

            LoadHtmlString(html, url);
        }

        public async void LoadUrl(string url)
        {
            try
            {
                var uri = new Uri(url);

                var safeHostUri = new Uri($"{uri.Scheme}://{uri.Authority}", UriKind.Absolute);
                var safeRelativeUri = new Uri($"{uri.PathAndQuery}{uri.Fragment}", UriKind.Relative);
                NSUrlRequest request = new NSUrlRequest(new Uri(safeHostUri, safeRelativeUri));

                if (!_firstLoadFinished && HasCookiesToLoad(url) && !Forms.IsiOS13OrNewer)
                {
                    _pendingUrl = url;
                    return;
                }

                _firstLoadFinished = true;
                await SyncNativeCookies(url);
                LoadRequest(request);
            }
            catch (UriFormatException formatException)
            {
                // If we got a format exception trying to parse the URI, it might be because
                // someone is passing in a local bundled file page. If we can find a better way
                // to detect that scenario, we should use it; until then, we'll fall back to 
                // local file loading here and see if that works:
                if (!LoadFile(url))
                {
                    Log.Warning(nameof(WkWebViewRenderer), $"Unable to Load Url {url}: {formatException}");
                }
            }
            catch (Exception exc)
            {
                Log.Warning(nameof(WkWebViewRenderer), $"Unable to Load Url {url}: {exc}");
            }
        }

        bool LoadFile(string url)
        {
            try
            {
                var file = Path.GetFileNameWithoutExtension(url);
                var ext = Path.GetExtension(url);

                var nsUrl = NSBundle.MainBundle.GetUrlForResource(file, ext);

                if (nsUrl == null)
                {
                    return false;
                }

                LoadFileUrl(nsUrl, nsUrl);

                return true;
            }
            catch (Exception ex)
            {
                Log.Warning(nameof(WkWebViewRenderer), $"Could not load {url} as local file: {ex}");
            }

            return false;
        }

        public void RegisterEffect(Effect effect)
        {
            VisualElementRenderer<VisualElement>.RegisterEffect(effect, this, NativeView);
        }

        public void UpdateCanGoBackForward()
        {
            ((ISuperWebViewController)WebView).CanGoBack = CanGoBack;
            ((ISuperWebViewController)WebView).CanGoForward = CanGoForward;
        }

        #endregion - Interface Methods

        #region - Internal Methods

        public override void MovedToWindow()
        {
            base.MovedToWindow();
            _firstLoadFinished = true;

            if (!string.IsNullOrWhiteSpace(_pendingUrl))
            {
                var closure = _pendingUrl;
                _pendingUrl = null;

                // I realize this looks like the worst hack ever but iOS 11 and cookies are super quirky
                // and this is the only way I could figure out how to get iOS 11 to inject a cookie 
                // the first time a WkWebView is used in your app. This only has to run the first time a WkWebView is used 
                // anywhere in the application. All subsequents uses of WkWebView won't hit this hack
                // Even if it's a WkWebView on a new page.
                // read through this thread https://developer.apple.com/forums/thread/99674
                // Or Bing "WkWebView and Cookies" to see the myriad of hacks that exist
                // Most of them all came down to different variations of synching the cookies before or after the
                // WebView is added to the controller. This is the only one I was able to make work
                // I think if we could delay adding the WebView to the Controller until after ViewWillAppear fires that might also work
                // But we're not really setup for that
                // If you'd like to try your hand at cleaning this up then UI Test Issue12134 and Issue3262 are your final bosses
                InvokeOnMainThread(async () =>
                {
                    await Task.Delay(500);
                    LoadUrl(closure);
                });
            }
        }

        void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == SuperWebView.SourceProperty.PropertyName)
                Load();
        }

        void Load()
        {
            if (_ignoreSourceChanges)
                return;

            if (WebView.Source == null)
                Log.Warning(nameof(SuperWkWebViewRenderer), "Webview source was null");

            if (WebView.Source != null)
                WebView.Source.Load(this);

            UpdateCanGoBackForward();
        }

        void OnEvalRequested(object sender, EvalRequested eventArg)
        {
            EvaluateJavaScriptAsync(eventArg.Script);
        }

        async Task<string> OnEvaluateJavaScriptRequested(string script)
        {
            //TODO: Use ConfigureAwait?
            var result = await EvaluateJavaScriptAsync(script);

            return result?.ToString();
        }

        void OnGoBackRequested(object sender, EventArgs eventArgs)
        {
            if (CanGoBack)
            {
                _lastBackForwardEvent = WebNavigationEvent.Back;
                GoBack();
            }

            UpdateCanGoBackForward();
        }

        void OnGoForwardRequested(object sender, EventArgs eventArgs)
        {
            if (CanGoForward)
            {
                _lastBackForwardEvent = WebNavigationEvent.Forward;
                GoForward();
            }

            UpdateCanGoBackForward();
        }

        async void OnReloadRequested(object sender, EventArgs eventArgs)
        {
            try
            {

                await SyncNativeCookies(Url?.AbsoluteUrl?.ToString());
            }
            catch (Exception exc)
            {
                Log.Warning(nameof(WkWebViewRenderer), $"Syncing Existing Cookies Failed: {exc}");
            }

            Reload();
        }

        #endregion - Internal Methods

        #region - Cleanup Methods

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _disposed = true;
            if (Interlocked.Decrement(ref _sharedPoolCount) == 0 && Forms.IsiOS12OrNewer)
                WKWebViewHelper.DisposeSharedPool();

            if (disposing)
            {
                if (IsLoading)
                    StopLoading();

                Element.PropertyChanged -= HandlePropertyChanged;
                WebView.EvalRequested -= OnEvalRequested;
                WebView.EvaluateJavaScriptRequested -= OnEvaluateJavaScriptRequested;
                WebView.GoBackRequested -= OnGoBackRequested;
                WebView.GoForwardRequested -= OnGoForwardRequested;
                WebView.ReloadRequested -= OnReloadRequested;

                Element?.ClearValue(Platform.RendererProperty);
                SetElement(null);

                _events?.Dispose();
                _tracker?.Dispose();
                _packager?.Dispose();

                _events = null;
                _tracker = null;
                _events = null;

                if (_disposables != null && _disposables.Any())
                    _disposables.ForEach(d => d.Dispose());
            }

            base.Dispose(disposing);
        }

        #endregion Cleanup Methods

        #endregion Methods

        #region Cookies - Move To Cookie Manager

        HashSet<string> _loadedCookies = new HashSet<string>();

        bool HasCookiesToLoad(string url)
        {
            var uri = CreateUriForCookies(url);

            if (uri == null)
                return false;

            var myCookieJar = WebView.Cookies;
            if (myCookieJar == null)
                return false;

            var cookies = myCookieJar.GetCookies(uri);
            if (cookies == null)
                return false;

            return cookies.Count > 0;
        }

        Uri CreateUriForCookies(string url)
        {
            if (url == null)
                return null;

            Uri uri;

            if (url.Length > 2000)
                url = url.Substring(0, 2000);

            if (Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                if (string.IsNullOrWhiteSpace(uri.Host))
                    return null;

                return uri;
            }

            return null;
        }

        public async Task SyncNativeCookies(string url)
        {
            var uri = CreateUriForCookies(url);

            if (uri == null)
                return;

            var myCookieJar = WebView.Cookies;
            if (myCookieJar == null)
                return;

            await InitialCookiePreloadIfNecessary(url);
            var cookies = myCookieJar.GetCookies(uri);
            if (cookies == null)
                return;

            var retrieveCurrentWebCookies = await GetCookiesFromNativeStore(url);

            List<NSHttpCookie> deleteCookies = new List<NSHttpCookie>();
            foreach (var cookie in retrieveCurrentWebCookies)
            {
                if (cookies[cookie.Name] != null)
                    continue;

                deleteCookies.Add(cookie);
            }

            List<Cookie> cookiesToSet = new List<Cookie>();
            foreach (Cookie cookie in cookies)
            {
                bool changeCookie = true;

                // This code is used to only push updates to cookies that have changed.
                // This doesn't quite work on on iOS 10 if we have to delete any cookies.
                // I haven't found a way on iOS 10 to remove individual cookies. 
                // The trick we use on Android with writing a cookie that expires doesn't work
                // So on iOS10 if the user wants to remove any cookies we just delete 
                // the cookie for the entire domain inside of DeleteCookies and then rewrite
                // all the cookies
                if (Forms.IsiOS11OrNewer || deleteCookies.Count == 0)
                {
                    foreach (var nsCookie in retrieveCurrentWebCookies)
                    {
                        // if the cookie value hasn't changed don't set it again
                        if (nsCookie.Domain == cookie.Domain &&
                            nsCookie.Name == cookie.Name &&
                            nsCookie.Value == cookie.Value)
                        {
                            changeCookie = false;
                            break;
                        }
                    }
                }

                if (changeCookie)
                    cookiesToSet.Add(cookie);
            }

            await SetCookie(cookiesToSet);
            await DeleteCookies(deleteCookies);
        }

        public async Task SyncNativeCookiesToElement(string url)
        {

        }

        async Task SetCookie(List<Cookie> cookies)
        {
            if (Forms.IsiOS11OrNewer)
            {
                foreach (var cookie in cookies)
                    await Configuration.WebsiteDataStore.HttpCookieStore.SetCookieAsync(new NSHttpCookie(cookie));
            }
            else
            {
                Configuration.UserContentController.RemoveAllUserScripts();

                if (cookies.Count > 0)
                {
                    WKUserScript wKUserScript = new WKUserScript(new NSString(GetCookieString(cookies)), WKUserScriptInjectionTime.AtDocumentStart, false);

                    Configuration.UserContentController.AddUserScript(wKUserScript);
                }
            }
        }

        string GetCookieString(List<Cookie> existingCookies)
        {
            StringBuilder cookieBuilder = new StringBuilder();
            foreach (System.Net.Cookie jCookie in existingCookies)
            {
                cookieBuilder.Append("document.cookie = '");
                cookieBuilder.Append(jCookie.Name);
                cookieBuilder.Append("=");

                if (jCookie.Expired)
                {
                    cookieBuilder.Append($"; Max-Age=0");
                    cookieBuilder.Append($"; expires=Sun, 31 Dec 2000 00:00:00 UTC");
                }
                else
                {
                    cookieBuilder.Append(jCookie.Value);
                    cookieBuilder.Append($"; Max-Age={jCookie.Expires.Subtract(DateTime.UtcNow).TotalSeconds}");
                }

                if (!String.IsNullOrWhiteSpace(jCookie.Domain))
                {
                    cookieBuilder.Append($"; Domain={jCookie.Domain}");
                }
                if (!String.IsNullOrWhiteSpace(jCookie.Domain))
                {
                    cookieBuilder.Append($"; Path={jCookie.Path}");
                }
                if (jCookie.Secure)
                {
                    cookieBuilder.Append($"; Secure");
                }
                if (jCookie.HttpOnly)
                {
                    cookieBuilder.Append($"; HttpOnly");
                }

                cookieBuilder.Append("';");
            }

            return cookieBuilder.ToString();
        }

        async Task DeleteCookies(List<NSHttpCookie> cookies)
        {
            if (Forms.IsiOS11OrNewer)
            {
                foreach (var cookie in cookies)
                    await Configuration.WebsiteDataStore.HttpCookieStore.DeleteCookieAsync(cookie);
            }
            else
            {
                var wKWebsiteDataStore = WKWebsiteDataStore.DefaultDataStore;

                // This is the only way I've found to delete cookies on pre ios 11
                // I tried to set an expired cookie but it doesn't delete the cookie
                // So, just deleting the whole domain is the best option I've found
                WKWebsiteDataStore
                    .DefaultDataStore
                    .FetchDataRecordsOfTypes(WKWebsiteDataStore.AllWebsiteDataTypes, (NSArray records) =>
                    {
                        for (nuint i = 0; i < records.Count; i++)
                        {
                            var record = records.GetItem<WKWebsiteDataRecord>(i);

                            foreach (var deleteme in cookies)
                            {
                                if (record.DisplayName.Contains(deleteme.Domain) || deleteme.Domain.Contains(record.DisplayName))
                                {
                                    WKWebsiteDataStore.DefaultDataStore.RemoveDataOfTypes(record.DataTypes,
                                          new[] { record }, () => { });

                                    break;
                                }

                            }
                        }
                    });
            }
        }

        async Task InitialCookiePreloadIfNecessary(string url)
        {
            var myCookieJar = WebView.Cookies;
            if (myCookieJar == null)
                return;

            var uri = CreateUriForCookies(url);

            if (uri == null)
                return;

            if (!_loadedCookies.Add(uri.Host))
                return;

            // pre ios 11 we sync cookies after navigated
            if (!Forms.IsiOS11OrNewer)
                return;

            var cookies = myCookieJar.GetCookies(uri);
            var existingCookies = await GetCookiesFromNativeStore(url);
            foreach (var nscookie in existingCookies)
            {
                if (cookies[nscookie.Name] == null)
                {
                    string cookieH = $"{nscookie.Name}={nscookie.Value}; domain={nscookie.Domain}; path={nscookie.Path}";
                    myCookieJar.SetCookies(uri, cookieH);
                }
            }
        }

        async Task<List<NSHttpCookie>> GetCookiesFromNativeStore(string url)
        {
            NSHttpCookie[] _initialCookiesLoaded = null;
            if (Forms.IsiOS11OrNewer)
            {
                _initialCookiesLoaded = await Configuration.WebsiteDataStore.HttpCookieStore.GetAllCookiesAsync();
            }
            else
            {
                // I haven't found a different way to get the cookies pre ios 11
                var cookieString = await WebView.EvaluateJavaScriptAsync("document.cookie");

                if (cookieString != null)
                {
                    CookieContainer extractCookies = new CookieContainer();
                    var uri = CreateUriForCookies(url);

                    foreach (var cookie in cookieString.Split(';'))
                        extractCookies.SetCookies(uri, cookie);

                    var extracted = extractCookies.GetCookies(uri);
                    _initialCookiesLoaded = new NSHttpCookie[extracted.Count];
                    for (int i = 0; i < extracted.Count; i++)
                    {
                        _initialCookiesLoaded[i] = new NSHttpCookie(extracted[i]);
                    }
                }
            }

            _initialCookiesLoaded = _initialCookiesLoaded ?? new NSHttpCookie[0];

            List<NSHttpCookie> existingCookies = new List<NSHttpCookie>();
            string domain = CreateUriForCookies(url).Host;
            foreach (var cookie in _initialCookiesLoaded)
            {
                // we don't care that much about this being accurate
                // the cookie container will split the cookies up more correctly
                if (!cookie.Domain.Contains(domain) && !domain.Contains(cookie.Domain))
                    continue;

                existingCookies.Add(cookie);
            }

            return existingCookies;
        }

        #endregion Cookies
    }
}
