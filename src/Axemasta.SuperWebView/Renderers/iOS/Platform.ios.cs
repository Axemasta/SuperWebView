using System;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.iOS;

namespace Axemasta.SuperWebView.iOS
{
    public class Platform
    {
		internal static readonly BindableProperty RendererProperty = BindableProperty.CreateAttached("Renderer", typeof(IVisualElementRenderer), typeof(Platform), default(IVisualElementRenderer),
			propertyChanged: (bindable, oldvalue, newvalue) =>
			{
#if DEBUG
				if (oldvalue != null && newvalue != null)
				{
					Log.Warning("Renderer", $"{bindable} already has a renderer attached to it: {oldvalue}. Please figure out why and then fix it.");
				}
#endif
				var view = bindable as VisualElement;
				if (view != null)
					view.IsPlatformEnabled = newvalue != null;
			});
	}
}
