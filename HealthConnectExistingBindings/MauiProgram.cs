using Microsoft.Extensions.Logging;
using HealthConnectExistingBindings.Services;
using HealthConnectExistingBindings.ViewModels;
using HealthConnectExistingBindings.Pages;

namespace HealthConnectExistingBindings
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Enregistrement du service de santé
            builder.Services.AddSingleton<IHealthService>(serviceProvider =>
            {
#if ANDROID
                // On Android, nous devrons passer l'Activity via un autre mécanisme
                // Pour l'instant, on utilise null et on l'initialisera plus tard
                var activity = Platform.CurrentActivity ?? throw new InvalidOperationException("Current activity is null");
                return HealthServiceFactory.CreateHealthService(activity);
#else
                return HealthServiceFactory.CreateHealthService();
#endif
            });

            // Enregistrement du ViewModel et de la page
            builder.Services.AddTransient<MainPageViewModel>();
            builder.Services.AddTransient<HealthViewModel>();
            builder.Services.AddTransient<HealthPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
