﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using Android.Content;
using Android.Webkit;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using AWebView = Android.Webkit.WebView;
using AMixedContentHandling = Android.Webkit.MixedContentHandling;

namespace Axemasta.SuperWebView.Droid
{
    public class SuperWebViewRenderer : ViewRenderer<SuperWebView, AWebView>, IWebViewDelegate
    {
        public const string AssetBaseUrl = "file:///android_asset/";

        WebNavigationEvent _eventState;
        WebViewClient _webViewClient;
        FormsSuperWebChromeClient _webChromeClient;
        bool _isDisposed = false;
        protected internal ISuperWebViewController ElementController => Element;
        protected internal bool IgnoreSourceChanges { get; set; }
        protected internal string UrlCanceled { get; set; }

        public SuperWebViewRenderer(Context context) : base(context)
        {
            AutoPackage = false;
        }

        public void LoadHtml(string html, string baseUrl)
        {
            _eventState = WebNavigationEvent.NewPage;
            Control.LoadDataWithBaseURL(baseUrl ?? AssetBaseUrl, html, "text/html", "UTF-8", null);
        }

        public void LoadUrl(string url)
        {
            LoadUrl(url, true);
        }

        async void LoadUrl(string url, bool fireNavigatingCanceled)
        {
            if (!fireNavigatingCanceled || !await SendNavigatingCanceledAsync(url))
            {
                _eventState = WebNavigationEvent.NewPage;
                Control.LoadUrl(url);
            }
        }

        protected internal async Task<bool> SendNavigatingCanceledAsync(string url)
        {
            if (Element == null || string.IsNullOrWhiteSpace(url))
                return true;

            if (url == AssetBaseUrl)
                return false;

            var args = new SuperWebNavigatingEventArgs(_eventState, new SuperUrlWebViewSource { Url = url }, url, true);
            SyncNativeCookies(url);
            ElementController.SendNavigating(args);
            UpdateCanGoBackForward();

            var cancel = false;

            if (args.DeferralRequested)
            {
                cancel = !await Task.Run(() => args.DeferredTask);
            }

            if (cancel)
                Console.WriteLine($"Navigation is being cancelled for url: {url}");

            return cancel;
        }

        protected override void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            if (disposing)
            {
                if (Element != null)
                {
                    Control?.StopLoading();

                    ElementController.EvalRequested -= OnEvalRequested;
                    ElementController.GoBackRequested -= OnGoBackRequested;
                    ElementController.GoForwardRequested -= OnGoForwardRequested;
                    ElementController.ReloadRequested -= OnReloadRequested;
                    ElementController.EvaluateJavaScriptRequested -= OnEvaluateJavaScriptRequested;

                    _webViewClient?.Dispose();
                    _webChromeClient?.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        protected virtual WebViewClient GetWebViewClient()
        {
            return new FormsSuperWebViewClient(this);
        }

        protected virtual FormsSuperWebChromeClient GetFormsWebChromeClient()
        {
            return new FormsSuperWebChromeClient();
        }

        protected override Size MinimumSize()
        {
            return new Size(Context.ToPixels(40), Context.ToPixels(40));
        }

        protected override AWebView CreateNativeControl()
        {
            var webView = new AWebView(Context);
            webView.Settings.SetSupportMultipleWindows(true);
            return webView;
        }

        internal WebNavigationEvent GetCurrentWebNavigationEvent()
        {
            return _eventState;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<SuperWebView> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                var webView = CreateNativeControl();
#pragma warning disable 618 // This can probably be replaced with LinearLayout(LayoutParams.MatchParent, LayoutParams.MatchParent); just need to test that theory
                webView.LayoutParameters = new global::Android.Widget.AbsoluteLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent, 0, 0);
#pragma warning restore 618

                _webViewClient = GetWebViewClient();
                webView.SetWebViewClient(_webViewClient);

                _webChromeClient = GetFormsWebChromeClient();
                _webChromeClient.SetContext(Context);
                webView.SetWebChromeClient(_webChromeClient);

                //if (Context.IsDesignerContext())
                //{
                //    SetNativeControl(webView);
                //    return;
                //}

                webView.Settings.JavaScriptEnabled = true;
                webView.Settings.DomStorageEnabled = true;
                SetNativeControl(webView);
            }

            if (e.OldElement != null)
            {
                var oldElementController = e.OldElement as ISuperWebViewController;
                oldElementController.EvalRequested -= OnEvalRequested;
                oldElementController.EvaluateJavaScriptRequested -= OnEvaluateJavaScriptRequested;
                oldElementController.GoBackRequested -= OnGoBackRequested;
                oldElementController.GoForwardRequested -= OnGoForwardRequested;
                oldElementController.ReloadRequested -= OnReloadRequested;
            }

            if (e.NewElement != null)
            {
                var newElementController = e.NewElement as ISuperWebViewController;
                newElementController.EvalRequested += OnEvalRequested;
                newElementController.EvaluateJavaScriptRequested += OnEvaluateJavaScriptRequested;
                newElementController.GoBackRequested += OnGoBackRequested;
                newElementController.GoForwardRequested += OnGoForwardRequested;
                newElementController.ReloadRequested += OnReloadRequested;

                UpdateMixedContentMode();
                UpdateEnableZoomControls();
                UpdateDisplayZoomControls();
            }

            Load();
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            switch (e.PropertyName)
            {
                case "Source":
                    Load();
                    break;
                case "MixedContentMode":
                    UpdateMixedContentMode();
                    break;
                case "EnableZoomControls":
                    UpdateEnableZoomControls();
                    break;
                case "DisplayZoomControls":
                    UpdateDisplayZoomControls();
                    break;
            }
        }

        HashSet<string> _loadedCookies = new HashSet<string>();

        Uri CreateUriForCookies(string url)
        {
            if (url == null)
                return null;

            Uri uri;

            if (url.Length > 2000)
                url = url.Substring(0, 2000);

            if (Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                if (String.IsNullOrWhiteSpace(uri.Host))
                    return null;

                return uri;
            }

            return null;
        }

        CookieCollection GetCookiesFromNativeStore(string url)
        {
            CookieContainer existingCookies = new CookieContainer();
            var cookieManager = CookieManager.Instance;
            var currentCookies = cookieManager.GetCookie(url);
            var uri = CreateUriForCookies(url);

            if (currentCookies != null)
            {
                foreach (var cookie in currentCookies.Split(';'))
                    existingCookies.SetCookies(uri, cookie);
            }

            return existingCookies.GetCookies(uri);
        }

        void InitialCookiePreloadIfNecessary(string url)
        {
            var myCookieJar = Element.Cookies;
            if (myCookieJar == null)
                return;

            var uri = CreateUriForCookies(url);
            if (uri == null)
                return;

            if (!_loadedCookies.Add(uri.Host))
                return;

            var cookies = myCookieJar.GetCookies(uri);

            if (cookies != null)
            {
                var existingCookies = GetCookiesFromNativeStore(url);
                foreach (Cookie cookie in existingCookies)
                {
                    if (cookies[cookie.Name] == null)
                        myCookieJar.Add(cookie);
                }
            }
        }

        internal void SyncNativeCookiesToElement(string url)
        {
            var myCookieJar = Element.Cookies;
            if (myCookieJar == null)
                return;

            var uri = CreateUriForCookies(url);
            if (uri == null)
                return;

            var cookies = myCookieJar.GetCookies(uri);
            var retrieveCurrentWebCookies = GetCookiesFromNativeStore(url);

            foreach (Cookie cookie in cookies)
            {
                var nativeCookie = retrieveCurrentWebCookies[cookie.Name];
                if (nativeCookie == null)
                    cookie.Expired = true;
                else
                    cookie.Value = nativeCookie.Value;
            }

            SyncNativeCookies(url);
        }

        void SyncNativeCookies(string url)
        {
            var uri = CreateUriForCookies(url);
            if (uri == null)
                return;

            var myCookieJar = Element.Cookies;
            if (myCookieJar == null)
                return;

            InitialCookiePreloadIfNecessary(url);
            var cookies = myCookieJar.GetCookies(uri);
            if (cookies == null)
                return;

            var retrieveCurrentWebCookies = GetCookiesFromNativeStore(url);

            var cookieManager = CookieManager.Instance;
            cookieManager.SetAcceptCookie(true);
            for (var i = 0; i < cookies.Count; i++)
            {
                var cookie = cookies[i];
                var cookieString = cookie.ToString();
                cookieManager.SetCookie(cookie.Domain, cookieString);
            }

            foreach (Cookie cookie in retrieveCurrentWebCookies)
            {
                if (cookies[cookie.Name] != null)
                    continue;

                var cookieString = $"{cookie.Name}=; max-age=0;expires=Sun, 31 Dec 2017 00:00:00 UTC";
                cookieManager.SetCookie(cookie.Domain, cookieString);
            }
        }

        void Load()
        {
            if (IgnoreSourceChanges)
                return;

            Element.Source?.Load(this);

            UpdateCanGoBackForward();
        }

        void OnEvalRequested(object sender, EvalRequested eventArg)
        {
            LoadUrl("javascript:" + eventArg.Script, false);
        }

        Task<string> OnEvaluateJavaScriptRequested(string script)
        {
            var jsr = new JavascriptResult();

            Control.EvaluateJavascript(script, jsr);

            return jsr.JsResult;
        }

        void OnGoBackRequested(object sender, EventArgs eventArgs)
        {
            if (Control.CanGoBack())
            {
                _eventState = WebNavigationEvent.Back;
                Control.GoBack();
            }

            UpdateCanGoBackForward();
        }

        void OnGoForwardRequested(object sender, EventArgs eventArgs)
        {
            if (Control.CanGoForward())
            {
                _eventState = WebNavigationEvent.Forward;
                Control.GoForward();
            }

            UpdateCanGoBackForward();
        }

        void OnReloadRequested(object sender, EventArgs eventArgs)
        {
            SyncNativeCookies(Control.Url?.ToString());
            _eventState = WebNavigationEvent.Refresh;
            Control.Reload();
        }

        protected internal void UpdateCanGoBackForward()
        {
            if (Element == null || Control == null)
                return;
            ElementController.CanGoBack = Control.CanGoBack();
            ElementController.CanGoForward = Control.CanGoForward();
        }

        void UpdateMixedContentMode()
        {
            if (Control != null && ((int)Forms.SdkInt >= 21))
            {
                Control.Settings.MixedContentMode = (AMixedContentHandling)Element.OnThisPlatform().MixedContentMode();
            }
        }

        void UpdateEnableZoomControls()
        {
            var value = Element.OnThisPlatform().ZoomControlsEnabled();
            Control.Settings.SetSupportZoom(value);
            Control.Settings.BuiltInZoomControls = value;
        }

        void UpdateDisplayZoomControls()
        {
            Control.Settings.DisplayZoomControls = Element.OnThisPlatform().ZoomControlsDisplayed();
        }

        class JavascriptResult : Java.Lang.Object, IValueCallback
        {
            TaskCompletionSource<string> source;
            public Task<string> JsResult { get { return source.Task; } }

            public JavascriptResult()
            {
                source = new TaskCompletionSource<string>();
            }

            public void OnReceiveValue(Java.Lang.Object result)
            {
                string json = ((Java.Lang.String)result).ToString();
                source.SetResult(json);
            }
        }
    }
}
