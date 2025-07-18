using HealthConnectExistingBindings.Services;
using System.ComponentModel;

namespace HealthConnectExistingBindings.ViewModels
{
    public class HealthViewModel : INotifyPropertyChanged
    {
        private readonly IHealthService _healthService;
        
        private int _currentSteps;
        private bool _isInitialized;
        private bool _hasPermissions;
        private string _statusMessage = "Initializing...";

        public HealthViewModel(IHealthService healthService)
        {
            _healthService = healthService ?? throw new ArgumentNullException(nameof(healthService));
            InitializeCommand = new Command(async () => await InitializeAsync());
            ReadStepsCommand = new Command(async () => await ReadStepsAsync());
            AddStepsCommand = new Command(async () => await AddStepsAsync(1000));
            RequestPermissionsCommand = new Command(async () => await RequestPermissionsAsync());
        }

        public int CurrentSteps
        {
            get => _currentSteps;
            set
            {
                _currentSteps = value;
                OnPropertyChanged();
            }
        }

        public bool IsInitialized
        {
            get => _isInitialized;
            set
            {
                _isInitialized = value;
                OnPropertyChanged();
            }
        }

        public bool HasPermissions
        {
            get => _hasPermissions;
            set
            {
                _hasPermissions = value;
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

        public Command InitializeCommand { get; }
        public Command ReadStepsCommand { get; }
        public Command AddStepsCommand { get; }
        public Command RequestPermissionsCommand { get; }

        private async Task InitializeAsync()
        {
            try
            {
                StatusMessage = "Initializing health service...";
                
                var initialized = await _healthService.InitializeAsync();
                IsInitialized = initialized;
                
                if (initialized)
                {
                    StatusMessage = "Health service initialized successfully";
                    await CheckPermissionsAsync();
                }
                else
                {
                    StatusMessage = "Failed to initialize health service";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error initializing: {ex.Message}";
            }
        }

        private async Task CheckPermissionsAsync()
        {
            try
            {
                StatusMessage = "Checking permissions...";
                var hasPermissions = await _healthService.HasAllPermissionsAsync();
                HasPermissions = hasPermissions;
                
                if (hasPermissions)
                {
                    StatusMessage = "All permissions granted";
                    await ReadStepsAsync();
                }
                else
                {
                    StatusMessage = "Permissions required. Please grant permissions.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error checking permissions: {ex.Message}";
            }
        }

        private async Task RequestPermissionsAsync()
        {
            try
            {
                StatusMessage = "Requesting permissions...";
                var granted = await _healthService.RequestPermissionsAsync();
                
                if (granted)
                {
                    await CheckPermissionsAsync();
                }
                else
                {
                    StatusMessage = "Permissions denied";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error requesting permissions: {ex.Message}";
            }
        }

        private async Task ReadStepsAsync()
        {
            try
            {
                StatusMessage = "Reading steps data...";
                var steps = await _healthService.ReadStepsTodayAsync();
                CurrentSteps = steps;
                StatusMessage = $"Steps read successfully: {steps}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error reading steps: {ex.Message}";
            }
        }

        private async Task AddStepsAsync(int steps)
        {
            try
            {
                StatusMessage = $"Adding {steps} steps...";
                var success = await _healthService.AddStepsTodayAsync(steps);
                
                if (success)
                {
                    StatusMessage = $"Successfully added {steps} steps";
                    await ReadStepsAsync(); // Refresh the current steps
                }
                else
                {
                    StatusMessage = "Failed to add steps";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error adding steps: {ex.Message}";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
