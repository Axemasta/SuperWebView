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

		readonly Lazy<PlatformConfigurationRegistry<SuperWebView>> _platformConfigurationRegistry;

		public SuperWebView()
        {
            _platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<SuperWebView>>(() => new PlatformConfigurationRegistry<SuperWebView>(this));
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
		public event EventHandler GoBackRequested;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler GoForwardRequested;

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
