using System.ComponentModel;
using Xamarin.Forms;

namespace Axemasta.SuperWebView
{
    public class SuperHtmlWebViewSource : SuperWebViewSource
    {
		public static readonly BindableProperty HtmlProperty = BindableProperty.Create(nameof(Html), typeof(string), typeof(SuperHtmlWebViewSource), default(string),
			propertyChanged: (bindable, oldvalue, newvalue) => ((SuperHtmlWebViewSource)bindable).OnSourceChanged());

		public static readonly BindableProperty BaseUrlProperty = BindableProperty.Create(nameof(BaseUrl), typeof(string), typeof(SuperHtmlWebViewSource), default(string),
			propertyChanged: (bindable, oldvalue, newvalue) => ((SuperHtmlWebViewSource)bindable).OnSourceChanged());

		public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(SuperHtmlWebViewSource), default(string),
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

		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public override void Load(ISuperWebViewDelegate renderer)
		{
			var title = Title;

			if (string.IsNullOrEmpty(title))
            {
				title = "Local File";
            }

			renderer.LoadHtml(Html, BaseUrl, title);
		}
	}
}
