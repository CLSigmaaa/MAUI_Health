using HealthConnectExistingBindings.Services;

#if ANDROID
using HealthConnectExistingBindings.Platforms.Android;
using Android.App;
#elif IOS
using HealthConnectExistingBindings.Platforms.iOS;
#endif

namespace HealthConnectExistingBindings.Services
{
    public static class HealthServiceFactory
    {
        public static IHealthService CreateHealthService(object? platformSpecificParameter = null)
        {
#if ANDROID
            if (platformSpecificParameter is Activity activity)
            {
                return new AndroidHealthService(activity);
            }
            else
            {
                throw new ArgumentException("Android platform requires an Activity parameter", nameof(platformSpecificParameter));
            }
#elif IOS
            return new iOSHealthService();
#else
            throw new PlatformNotSupportedException("Health services are only supported on Android and iOS platforms");
#endif
        }
    }
}
