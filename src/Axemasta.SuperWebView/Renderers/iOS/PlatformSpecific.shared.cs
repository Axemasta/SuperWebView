using Xamarin.Forms;
using FormsElement = Axemasta.SuperWebView.SuperWebView;
using _iOS = Xamarin.Forms.PlatformConfiguration.iOS; // iOS is a namespace hence the _

namespace Axemasta.SuperWebView.PlatformConfiguration.iOSSpecific
{
    public static class iOSConfiguration
    {
        #region Allows Link Preview

        public static readonly BindableProperty AllowsLinkPreviewProperty = BindableProperty.Create("AllowLinkPreview", typeof(bool), typeof(iOSConfiguration), false);

        public static IPlatformElementConfiguration<_iOS, FormsElement> SetAllowsLinkPreview(this IPlatformElementConfiguration<_iOS, FormsElement> config, bool value)
        {
            SetAllowsLinkPreview(config.Element, value);
            return config;
        }

        public static void SetAllowsLinkPreview(BindableObject element, bool value)
        {
            element.SetValue(AllowsLinkPreviewProperty, value);
        }

        public static bool GetAllowsLinkPreview(BindableObject element)
        {
            return (bool)element.GetValue(AllowsLinkPreviewProperty);
        }

        #endregion Allows Link Preview
    }
}
