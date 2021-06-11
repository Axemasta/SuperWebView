using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Axemasta.SuperWebView.Internals;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Axemasta.SuperWebView
{
	/// <summary>
    /// Super Web View
    /// </summary>
    [RenderWith(typeof(_SuperWebViewRenderer))]
    public class SuperWebView : View, ISuperWebViewController, IElementConfiguration<SuperWebView>
    {
        public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(SuperWebViewSource), typeof(SuperWebView), default(SuperWebViewSource),
            propertyChanging: (bindable, oldvalue, newvalue) =>
            {
                var source = oldvalue as SuperWebViewSource;
                if (source != null)
                    source.SourceChanged -= ((SuperWebView)bindable).OnSourceChanged;
            }, propertyChanged: (bindable, oldvalue, newvalue) =>
            {
                var source = newvalue as SuperWebViewSource;
                var webview = (SuperWebView)bindable;
                if (source != null)
                {
                    source.SourceChanged += webview.OnSourceChanged;
                    SetInheritedBindingContext(source, webview.BindingContext);
                }
            });

        static readonly BindablePropertyKey CanGoBackPropertyKey = BindableProperty.CreateReadOnly(nameof(CanGoBack), typeof(bool), typeof(SuperWebView), false);

        public static readonly BindableProperty CanGoBackProperty = CanGoBackPropertyKey.BindableProperty;

        static readonly BindablePropertyKey CanGoForwardPropertyKey = BindableProperty.CreateReadOnly(nameof(CanGoForward), typeof(bool), typeof(SuperWebView), false);

        public static readonly BindableProperty CanGoForwardProperty = CanGoForwardPropertyKey.BindableProperty;

        public static readonly BindableProperty CookiesProperty = BindableProperty.Create(nameof(Cookies), typeof(CookieContainer), typeof(SuperWebView), null);

		static readonly BindablePropertyKey ProgressPropertyKey = BindableProperty.CreateReadOnly(nameof(Progress), typeof(double), typeof(SuperWebView), (double)0);

		public static readonly BindableProperty ProgressProperty = ProgressPropertyKey.BindableProperty;

		readonly Lazy<PlatformConfigurationRegistry<SuperWebView>> _platformConfigurationRegistry;

		public SuperWebView()
        {
            _platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<SuperWebView>>(() => new PlatformConfigurationRegistry<SuperWebView>(this));

            this.RendererInitialised += OnRendererInitialised;
        }

        public IPlatformElementConfiguration<T, SuperWebView> On<T>() where T : IConfigPlatform
        {
            return _platformConfigurationRegistry.Value.On<T>();
        }

		[EditorBrowsable(EditorBrowsableState.Never)]
		bool ISuperWebViewController.CanGoBack
		{
			get { return CanGoBack; }
			set { SetValue(CanGoBackPropertyKey, value); }
		}

		public bool CanGoBack
		{
			get { return (bool)GetValue(CanGoBackProperty); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		bool ISuperWebViewController.CanGoForward
		{
			get { return CanGoForward; }
			set { SetValue(CanGoForwardPropertyKey, value); }
		}

		public bool CanGoForward
		{
			get { return (bool)GetValue(CanGoForwardProperty); }
		}

		public CookieContainer Cookies
		{
			get { return (CookieContainer)GetValue(CookiesProperty); }
			set { SetValue(CookiesProperty, value); }
		}

		[Xamarin.Forms.TypeConverter(typeof(SuperWebViewSourceTypeConverter))]
		public SuperWebViewSource Source
		{
			get { return (SuperWebViewSource)GetValue(SourceProperty); }
			set { SetValue(SourceProperty, value); }
		}

		public double Progress
		{
			get => (double)GetValue(ProgressProperty);
			set => SetValue(ProgressProperty, value);
		}

		public void Eval(string script)
		{
			EventHandler<EvalRequested> handler = EvalRequested;
			handler?.Invoke(this, new EvalRequested(script));
		}

		public async Task<string> EvaluateJavaScriptAsync(string script)
		{
			EvaluateJavaScriptDelegate handler = EvaluateJavaScriptRequested;

			if (script == null)
				return null;

			//make all the platforms mimic Android's implementation, which is by far the most complete.
			if (Xamarin.Forms.Device.RuntimePlatform != "Android")
			{
				script = EscapeJsString(script);
				script = "try{JSON.stringify(eval('" + script + "'))}catch(e){'null'};";
			}

			var result = await handler?.Invoke(script);

			//if the js function errored or returned null/undefined treat it as null
			if (result == "null")
				result = null;

			//JSON.stringify wraps the result in literal quotes, we just want the actual returned result
			//note that if the js function returns the string "null" we will get here and not above
			else if (result != null)
				result = result.Trim('"');

			return result;
		}

		public void GoBack()
			=> GoBackRequested?.Invoke(this, EventArgs.Empty);

		public void GoForward()
			=> GoForwardRequested?.Invoke(this, EventArgs.Empty);

		public void Reload()
			=> ReloadRequested?.Invoke(this, EventArgs.Empty);

		public event EventHandler<SuperWebNavigatedEventArgs> Navigated;

		public event EventHandler<SuperWebNavigatingEventArgs> Navigating;

		public event EventHandler<NavigationCancelledEventArgs> NavigationCancelled;

		/// <summary>
        /// Called When WebView Javascript Calls Back To The App
        /// </summary>
		public event EventHandler<BrowserInvocationEventArgs> BrowserInvocation;

		public void SendBrowserInvocation(BrowserInvocationEventArgs args)
        {
			BrowserInvocation?.Invoke(this, args);
        }

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();

			SuperWebViewSource source = Source;

			if (source != null)
			{
				SetInheritedBindingContext(source, BindingContext);
			}
		}

		protected override void OnPropertyChanged(string propertyName)
		{
			if (propertyName == nameof(BindingContext))
			{
				SuperWebViewSource source = Source;
				if (source != null)
					SetInheritedBindingContext(source, BindingContext);
			}

			base.OnPropertyChanged(propertyName);
		}

		protected void OnSourceChanged(object sender, EventArgs e)
		{
			OnPropertyChanged(SourceProperty.PropertyName);
		}

		event EventHandler<EvalRequested> ISuperWebViewController.EvalRequested
		{
			add { EvalRequested += value; }
			remove { EvalRequested -= value; }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler<EvalRequested> EvalRequested;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EvaluateJavaScriptDelegate EvaluateJavaScriptRequested;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event InjectJavaScriptDelegate InjectJavaScriptRequested;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler GoBackRequested;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler GoForwardRequested;

		public event EventHandler<ProgressEventArgs> ProgressChanged;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendNavigated(SuperWebNavigatedEventArgs args)
		{
			Navigated?.Invoke(this, args);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendNavigating(SuperWebNavigatingEventArgs args)
		{
			Navigating?.Invoke(this, args);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler ReloadRequested;

		/// <summary>
        /// Send Progress Update
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="maximum"></param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendProgressChanged(ProgressEventArgs args)
        {
			ProgressChanged?.Invoke(this, args);
        }

		/// <summary>
        /// Send Navigation Cancelled
        /// </summary>
        /// <param name="args"></param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendNavigationCancelled(NavigationCancelledEventArgs args)
        {
			NavigationCancelled?.Invoke(this, args);
        }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler RendererInitialised;

		/// <summary>
        /// Inject Javascript Into The DOM
        /// </summary>
        /// <param name="scripts"></param>
		public void InjectJavascript(List<JavaScript> scripts)
        {
			if (scripts is null || scripts.Count < 1)
				return;

			if (!_rendererInitialised)
            {
				// Need to wait for SetElement to finish setting up before the handler is available for use
				RendererInitialised += (s, e) => OnInjectJavascript(scripts);
			}
        }

		private bool _rendererInitialised;

		private void OnRendererInitialised(object sender, EventArgs e)
		{
			this.RendererInitialised -= OnRendererInitialised;
			_rendererInitialised = true;
		}

		private void OnInjectJavascript(List<JavaScript> scripts)
        {
			RendererInitialised -= (s, e) => OnInjectJavascript(scripts);

			InjectJavaScriptDelegate handler = InjectJavaScriptRequested;

			foreach (var script in scripts)
			{
				// load script
				var loaded = script.TryLoadScript(out string scriptContent);

				if (!loaded)
				{
					// Warn
					Log.Warning(nameof(SuperWebView), $"Unable to load script content for: {script.Name}");
					continue;
				}

				// Send load to renderer
				handler?.Invoke(scriptContent);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendRendererInitialised()
        {
			this.RendererInitialised?.Invoke(this, EventArgs.Empty);
        }

		static string EscapeJsString(string js)
		{
			if (js == null)
				return null;

			if (!js.Contains("'"))
				return js;

			//get every quote in the string along with all the backslashes preceding it
			var singleQuotes = Regex.Matches(js, @"(\\*?)'");

			var uniqueMatches = new List<string>();

			for (var i = 0; i < singleQuotes.Count; i++)
			{
				var matchedString = singleQuotes[i].Value;
				if (!uniqueMatches.Contains(matchedString))
				{
					uniqueMatches.Add(matchedString);
				}
			}

			uniqueMatches.Sort((x, y) => y.Length.CompareTo(x.Length));

			//escape all quotes from the script as well as add additional escaping to all quotes that were already escaped
			for (var i = 0; i < uniqueMatches.Count; i++)
			{
				var match = uniqueMatches[i];
				var numberOfBackslashes = match.Length - 1;
				var slashesToAdd = (numberOfBackslashes * 2) + 1;
				var replacementStr = "'".PadLeft(slashesToAdd + 1, '\\');
				js = Regex.Replace(js, @"(?<=[^\\])" + Regex.Escape(match), replacementStr);
			}

			return js;
		}
	}
}
