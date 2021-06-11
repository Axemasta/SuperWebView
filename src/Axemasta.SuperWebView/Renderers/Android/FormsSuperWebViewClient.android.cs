using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Runtime;
using Android.Webkit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using WView = Android.Webkit.WebView;

namespace Axemasta.SuperWebView.Droid
{
	public class FormsSuperWebViewClient : WebViewClient
	{
		private readonly List<string> _scripts;
		WebNavigationResult _navigationResult = WebNavigationResult.Success;
		SuperWebViewRenderer _renderer;
		string _lastUrlNavigatedCancel;

		public FormsSuperWebViewClient(SuperWebViewRenderer renderer)
        {
			_renderer = renderer ?? throw new ArgumentNullException("renderer");
			_scripts = new List<string>();
		}

		protected FormsSuperWebViewClient(IntPtr javaReference, JniHandleOwnership transfer)
			: base(javaReference, transfer)
		{
			_scripts = new List<string>();
		}

		async Task<bool> SendNavigatingCanceledAsync(string url)
		{
			if (_renderer == null)
				return true;

			return await _renderer.SendNavigatingCanceledAsync(url);
		}

		[Obsolete("ShouldOverrideUrlLoading(view,url) is obsolete as of version 4.0.0. This method was deprecated in API level 24.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		// api 19-23
		public override bool ShouldOverrideUrlLoading(WView view, string url)
        {
			OverrideUrlLoading(url, () => SendNavigatingCanceledAsync(url));

			return true;
		}

		// api 24+
		public override bool ShouldOverrideUrlLoading(WView view, IWebResourceRequest request)
        {
			var url = request?.Url?.ToString();

			OverrideUrlLoading(url, () => SendNavigatingCanceledAsync(url));

			return true;
		}

		public override async void OnPageStarted(WView view, string url, Bitmap favicon)
		{
			if (_renderer?.Element == null || string.IsNullOrWhiteSpace(url) || url == WebViewRenderer.AssetBaseUrl)
				return;

			_renderer.SyncNativeCookiesToElement(url);
			var cancel = false;

			if (!url.Equals(_renderer.UrlCanceled, StringComparison.OrdinalIgnoreCase))
			{
				cancel = await SendNavigatingCanceledAsync(url);
			}

			_renderer.UrlCanceled = null;

			if (cancel)
			{
				_navigationResult = WebNavigationResult.Cancel;
				view.StopLoading();
			}
			else
			{
				_navigationResult = WebNavigationResult.Success;
				base.OnPageStarted(view, url, favicon);
			}
		}

		public override void OnPageFinished(WView view, string url)
		{
			if (_renderer?.Element == null || url == WebViewRenderer.AssetBaseUrl)
				return;

			var source = new SuperUrlWebViewSource { Url = url };
			_renderer.IgnoreSourceChanges = true;
			_renderer.ElementController.SetValueFromRenderer(SuperWebView.SourceProperty, source);
			_renderer.IgnoreSourceChanges = false;

			bool navigate = _navigationResult == WebNavigationResult.Failure ? !url.Equals(_lastUrlNavigatedCancel, StringComparison.OrdinalIgnoreCase) : true;
			_lastUrlNavigatedCancel = _navigationResult == WebNavigationResult.Cancel ? url : null;

			if (navigate)
			{
				var args = new SuperWebNavigatedEventArgs(_renderer.GetCurrentWebNavigationEvent(), source, url, _navigationResult);
				_renderer.SyncNativeCookiesToElement(url);
				_renderer.ElementController.SendNavigated(args);
			}

			if (_scripts != null && _scripts.Count > 0)
			{
				foreach (string script in _scripts)
				{
					view.EvaluateJavascript(script, null);
				}
			}

			_renderer.UpdateCanGoBackForward();

			base.OnPageFinished(view, url);
		}

		[Obsolete("OnReceivedError is obsolete as of version 2.3.0. This method was deprecated in API level 23.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override void OnReceivedError(WView view, ClientError errorCode, string description, string failingUrl)
		{
			if (failingUrl == _renderer?.Control.Url)
			{
				_navigationResult = WebNavigationResult.Failure;
				if (errorCode == ClientError.Timeout)
					_navigationResult = WebNavigationResult.Timeout;
			}
#pragma warning disable 618
			base.OnReceivedError(view, errorCode, description, failingUrl);
#pragma warning restore 618
		}

		public override void OnReceivedError(WView view, IWebResourceRequest request, WebResourceError error)
		{
			if (request.Url.ToString() == _renderer?.Control.Url)
			{
				_navigationResult = WebNavigationResult.Failure;
				if (error.ErrorCode == ClientError.Timeout)
					_navigationResult = WebNavigationResult.Timeout;
			}
			base.OnReceivedError(view, request, error);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
				_renderer = null;
		}

		public async void OverrideUrlLoading(string url, Func<Task<bool>> urlEvaluator)
		{
			if (urlEvaluator == null)
			{
				_renderer.LoadUrl(url);
				return;
			}

			var canload = !await urlEvaluator.Invoke();

			if (canload)
			{
				_renderer.LoadUrl(url);
			}
		}

		public void InjectScript(string script)
        {
			_scripts.Add(script);
        }
	}
}
