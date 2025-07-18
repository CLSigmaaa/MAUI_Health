
using HealthConnectExistingBindings.ViewModels;

namespace HealthConnectExistingBindings
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
