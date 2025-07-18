using HealthConnectExistingBindings.ViewModels;

namespace HealthConnectExistingBindings.Pages;

public partial class HealthPage : ContentPage
{
    public HealthPage(HealthViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
