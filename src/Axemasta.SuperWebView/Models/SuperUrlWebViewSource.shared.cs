using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace Axemasta.SuperWebView
{
    public class SuperUrlWebViewSource : SuperWebViewSource
	{
		public static readonly BindableProperty UrlProperty = BindableProperty.Create(nameof(Url), typeof(string), typeof(SuperUrlWebViewSource), default(string),
			propertyChanged: (bindable, oldvalue, newvalue) => ((SuperUrlWebViewSource)bindable).OnSourceChanged());

		public string Url
		{
			get { return (string)GetValue(UrlProperty); }
			set { SetValue(UrlProperty, value); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public override void Load(ISuperWebViewDelegate renderer)
		{
			renderer.LoadUrl(Url);
		}
	}
}
