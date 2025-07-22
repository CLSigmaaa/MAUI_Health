using HealthConnectExistingBindings.Services;
using Microsoft.Maui.ApplicationModel;
using System.ComponentModel;

namespace HealthConnectExistingBindings.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private readonly IHealthService _healthService;
        
        private int _steps;
        private bool _isLoading;
        private string _statusMessage = "Appuyez sur Actualiser pour actualiser le nombre de pas";

        public MainPageViewModel(IHealthService healthService)
        {
            _healthService = healthService ?? throw new ArgumentNullException(nameof(healthService));
            RefreshCommand = new Command(async () => await RefreshStepsAsync());
            
            // Initialiser automatiquement au démarrage
            _ = InitializeAsync();
        }

        public int Steps
        {
            get => _steps;
            set
            {
                _steps = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public Command RefreshCommand { get; }

        private async Task InitializeAsync()
        {
            try
            {
                StatusMessage = "Initialisation du service de santé...";
                
                var initialized = await _healthService.InitializeAsync();
                if (!initialized)
                {
                    StatusMessage = "Impossible d'initialiser le service de santé";
                    return;
                }

                var hasPermissions = await _healthService.HasAllPermissionsAsync();
                if (!hasPermissions)
                {
                    StatusMessage = "Demande des permissions...";
                    //await _healthService.RequestPermissionsAsync();

                }

                await RefreshStepsAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur d'initialisation: {ex.Message}";
            }
        }

        private async Task RefreshStepsAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Chargement des données de pas...";
                
                var steps = await _healthService.ReadStepsTodayAsync();
                Steps = steps;
                StatusMessage = $"Dernière mise à jour: {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
