using UIKit;

namespace Axemasta.SuperWebView.iOS
{
    public static class Forms
    {
		static bool? s_isiOS11OrNewer;
		static bool? s_isiOS12OrNewer;
		static bool? s_isiOS13OrNewer;

		internal static bool IsiOS11OrNewer
		{
			get
			{
				if (!s_isiOS11OrNewer.HasValue)
					s_isiOS11OrNewer = UIDevice.CurrentDevice.CheckSystemVersion(11, 0);
				return s_isiOS11OrNewer.Value;
			}
		}

		internal static bool IsiOS12OrNewer
		{
			get
			{
				if (!s_isiOS12OrNewer.HasValue)
					s_isiOS12OrNewer = UIDevice.CurrentDevice.CheckSystemVersion(12, 0);
				return s_isiOS12OrNewer.Value;
			}
		}

		internal static bool IsiOS13OrNewer
		{
			get
			{
				if (!s_isiOS13OrNewer.HasValue)
					s_isiOS13OrNewer = UIDevice.CurrentDevice.CheckSystemVersion(13, 0);
				return s_isiOS13OrNewer.Value;
			}
		}
	}
}
