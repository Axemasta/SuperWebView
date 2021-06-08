using System;
using Xamarin.Forms;

namespace Axemasta.SuperWebView
{
	[Xamarin.Forms.Xaml.TypeConversion(typeof(SuperUrlWebViewSource))]
	public class SuperWebViewSourceTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value != null)
				return new UrlWebViewSource { Url = value };

			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(SuperUrlWebViewSource)));
		}

		public override string ConvertToInvariantString(object value)
		{
			if (!(value is SuperUrlWebViewSource uwvs))
				throw new NotSupportedException();
			return uwvs.Url;
		}
	}
}
