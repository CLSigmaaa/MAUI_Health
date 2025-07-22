using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Activity.Result;
using AndroidX.Health.Connect.Client;
using HealthConnectExistingBindings.Platforms.Android;
using HealthConnectExistingBindings.Services;

namespace HealthConnectExistingBindings
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        private IHealthService? _healthService;
        private ActivityResultLauncher _requestPermissionsLauncher;


        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Console.WriteLine("MainActivity OnCreate called");

            _requestPermissionsLauncher = RegisterForActivityResult(
                PermissionController.CreateRequestPermissionResultContract(),
                new PermissionResultCallback()
            );

            _healthService = HealthServiceFactory.CreateHealthService(this);
        }

        public void AskPermissions(HashSet<string> permissions)
        {
            var requestPermissionActivityContract = PermissionController.CreateRequestPermissionResultContract();
            //var requestPermissions = RegisterForActivityResult(requestPermissionActivityContract, new PermissionResultCallback());

            Console.WriteLine("Launching permission request...");

            // Convert HashSet<string> to Java.Util.HashSet for Launch
            var javaPermissions = new Java.Util.HashSet();
            foreach (var perm in permissions)
            {
                javaPermissions.Add(perm);
                Console.WriteLine($"Adding missing permission: {perm}");
            }

            _requestPermissionsLauncher.Launch(javaPermissions);
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
                    var permissionsGranted = _healthService.RequestPermissionsAsync();
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

