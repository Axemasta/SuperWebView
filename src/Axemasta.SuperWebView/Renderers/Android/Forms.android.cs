using System;
using Android.OS;

namespace Axemasta.SuperWebView.Droid
{
    public static class Forms
    {
		static BuildVersionCodes? s_sdkInt;

		internal static BuildVersionCodes SdkInt
		{
			get
			{
				if (!s_sdkInt.HasValue)
					s_sdkInt = Build.VERSION.SdkInt;
				return (BuildVersionCodes)s_sdkInt;
			}
		}
    }
}
