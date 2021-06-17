namespace Axemasta.SuperWebView.PlatformConfiguration.iOSSpecific
{
    using Xamarin.Forms;
    using FormsElement = Axemasta.SuperWebView.SuperWebView;
    using iOS = Xamarin.Forms.PlatformConfiguration.iOS;

    public static class SuperWebView
    {
        public static readonly BindableProperty AllowsLinkPreviewProperty = BindableProperty.Create("AllowLinkPreview", typeof(bool), typeof(SuperWebView), false);

        public static IPlatformElementConfiguration<iOS, FormsElement> SetAllowsLinkPreview(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
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
    }
}
