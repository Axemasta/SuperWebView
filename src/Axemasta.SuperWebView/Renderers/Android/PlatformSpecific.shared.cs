using Xamarin.Forms;

namespace Axemasta.SuperWebView.PlatformConfiguration.AndroidSpecific
{
    using FormsElement = Axemasta.SuperWebView.SuperWebView;
	using AndroidPlatform = Xamarin.Forms.PlatformConfiguration.Android;

	public enum MixedContentHandling
	{
		AlwaysAllow = 0,
		NeverAllow = 1,
		CompatibilityMode = 2
	}

	public static class AndroidConfiguration
	{
        #region Mixed Content Mode

        public static readonly BindableProperty MixedContentModeProperty = BindableProperty.Create("MixedContentMode", typeof(MixedContentHandling), typeof(SuperWebView), MixedContentHandling.NeverAllow);

		public static MixedContentHandling GetMixedContentMode(BindableObject element)
		{
			return (MixedContentHandling)element.GetValue(MixedContentModeProperty);
		}

		public static void SetMixedContentMode(BindableObject element, MixedContentHandling value)
		{
			element.SetValue(MixedContentModeProperty, value);
		}

		public static MixedContentHandling MixedContentMode(this IPlatformElementConfiguration<AndroidPlatform, FormsElement> config)
		{
			return GetMixedContentMode(config.Element);
		}

		public static IPlatformElementConfiguration<AndroidPlatform, FormsElement> SetMixedContentMode(this IPlatformElementConfiguration<AndroidPlatform, FormsElement> config, MixedContentHandling value)
		{
			SetMixedContentMode(config.Element, value);
			return config;
		}

        #endregion Mixed Content Mode

        #region Enable Zoom Controls

        public static readonly BindableProperty EnableZoomControlsProperty = BindableProperty.Create("EnableZoomControls", typeof(bool), typeof(FormsElement), false);

		public static bool GetEnableZoomControls(FormsElement element)
		{
			return (bool)element.GetValue(EnableZoomControlsProperty);
		}

		public static void SetEnableZoomControls(FormsElement element, bool value)
		{
			element.SetValue(EnableZoomControlsProperty, value);
		}

		public static void EnableZoomControls(this IPlatformElementConfiguration<AndroidPlatform, FormsElement> config, bool value)
		{
			SetEnableZoomControls(config.Element, value);
		}
		public static bool ZoomControlsEnabled(this IPlatformElementConfiguration<AndroidPlatform, FormsElement> config)
		{
			return GetEnableZoomControls(config.Element);
		}

		public static IPlatformElementConfiguration<AndroidPlatform, FormsElement> SetEnableZoomControls(this IPlatformElementConfiguration<AndroidPlatform, FormsElement> config, bool value)
		{
			SetEnableZoomControls(config.Element, value);
			return config;
		}

        #endregion Enable Zoom Controls

        #region Display Zoom Controls

        public static readonly BindableProperty DisplayZoomControlsProperty = BindableProperty.Create("DisplayZoomControls", typeof(bool), typeof(FormsElement), true);

		public static bool GetDisplayZoomControls(FormsElement element)
		{
			return (bool)element.GetValue(DisplayZoomControlsProperty);
		}

		public static void SetDisplayZoomControls(FormsElement element, bool value)
		{
			element.SetValue(DisplayZoomControlsProperty, value);
		}

		public static void DisplayZoomControls(this IPlatformElementConfiguration<AndroidPlatform, FormsElement> config, bool value)
		{
			SetDisplayZoomControls(config.Element, value);
		}

		public static bool ZoomControlsDisplayed(this IPlatformElementConfiguration<AndroidPlatform, FormsElement> config)
		{
			return GetDisplayZoomControls(config.Element);
		}

		public static IPlatformElementConfiguration<AndroidPlatform, FormsElement> SetDisplayZoomControls(this IPlatformElementConfiguration<AndroidPlatform, FormsElement> config, bool value)
		{
			SetDisplayZoomControls(config.Element, value);
			return config;
		}

		#endregion Display Zoom Controls

		#region Hardening Enabled

		public static readonly BindableProperty HardeningEnabledProperty = BindableProperty.Create("HardeningEnabled", typeof(bool), typeof(FormsElement), true);

		public static IPlatformElementConfiguration<AndroidPlatform, FormsElement> SetHardeningEnabled(this IPlatformElementConfiguration<AndroidPlatform, FormsElement> config, bool value)
		{
			SetHardeningEnabled(config.Element, value);
			return config;
		}

		public static void SetHardeningEnabled(BindableObject element, bool value)
		{
			element.SetValue(HardeningEnabledProperty, value);
		}

		public static bool GetHardeningEnabled(BindableObject element)
		{
			return (bool)element.GetValue(HardeningEnabledProperty);
		}

		#endregion Hardening Enabled
	}
}
