using System.ComponentModel;
using Xamarin.Forms;

namespace Axemasta.SuperWebView
{
    public class SuperHtmlWebViewSource : SuperWebViewSource
    {
		public static readonly BindableProperty HtmlProperty = BindableProperty.Create(nameof(Html), typeof(string), typeof(HtmlWebViewSource), default(string),
			propertyChanged: (bindable, oldvalue, newvalue) => ((SuperHtmlWebViewSource)bindable).OnSourceChanged());

		public static readonly BindableProperty BaseUrlProperty = BindableProperty.Create(nameof(BaseUrl), typeof(string), typeof(HtmlWebViewSource), default(string),
			propertyChanged: (bindable, oldvalue, newvalue) => ((SuperHtmlWebViewSource)bindable).OnSourceChanged());

		public string BaseUrl
		{
			get { return (string)GetValue(BaseUrlProperty); }
			set { SetValue(BaseUrlProperty, value); }
		}

		public string Html
		{
			get { return (string)GetValue(HtmlProperty); }
			set { SetValue(HtmlProperty, value); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public override void Load(IWebViewDelegate renderer)
		{
			renderer.LoadHtml(Html, BaseUrl);
		}
	}
}
