using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace Axemasta.SuperWebView
{
    public abstract class SuperWebViewSource : BindableObject
	{
		public static implicit operator SuperWebViewSource(Uri url)
		{
			return new SuperUrlWebViewSource { Url = url?.AbsoluteUri };
		}

		public static implicit operator SuperWebViewSource(string url)
		{
			return new SuperUrlWebViewSource { Url = url };
		}

		protected void OnSourceChanged()
		{
			EventHandler eh = SourceChanged;
			if (eh != null)
				eh(this, EventArgs.Empty);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public abstract void Load(ISuperWebViewDelegate renderer);

		internal event EventHandler SourceChanged;
	}
}
