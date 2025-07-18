using Android.App;
using Android.Content.PM;
using Android.OS;
using HealthConnectExistingBindings.Services;

namespace HealthConnectExistingBindings
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        private IHealthService? _healthService;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Console.WriteLine("MainActivity OnCreate called");

            // Créer le service de santé Android
            _healthService = HealthServiceFactory.CreateHealthService(this);

            // Initialiser et utiliser le service
            _ = InitializeHealthServiceAsync();
        }

        private async Task InitializeHealthServiceAsync()
        {
            try
            {
                // Initialiser le service
                var initialized = await _healthService!.InitializeAsync();
                if (!initialized)
                {
                    Console.WriteLine("Failed to initialize health service");
                    return;
                }

                // Vérifier les permissions
                var hasPermissions = await _healthService.HasAllPermissionsAsync();
                if (!hasPermissions)
                {
                    // Demander les permissions
                    var permissionsGranted = await _healthService.RequestPermissionsAsync();
                    Console.WriteLine($"Permissions granted: {permissionsGranted}");
                }

                // Effectuer les opérations de test
                await TestHealthOperationsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in InitializeHealthServiceAsync: {ex.Message}");
            }
        }

        private async Task TestHealthOperationsAsync()
        {
            try
            {
                if (_healthService == null) return;

                // Lire les pas d'aujourd'hui
                var currentSteps = await _healthService.ReadStepsTodayAsync();
                Console.WriteLine($"Current steps today: {currentSteps}");

                // Ajouter 1000 pas
                // var added = await _healthService.AddStepsTodayAsync(1000);
                // Console.WriteLine($"Added 1000 steps: {added}");

                // Relire les pas
                var newSteps = await _healthService.ReadStepsTodayAsync();
                Console.WriteLine($"New steps total: {newSteps}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in TestHealthOperationsAsync: {ex.Message}");
            }
        }
    }
}

