using MedicalResultsTracker.ViewModel;

namespace MedicalResultsTracker.View
{
    public partial class PressurePage : ContentPage
    {
        private readonly PressureViewModel _viewModel;

        public PressurePage(PressureViewModel viewModel)
        {
            InitializeComponent();

            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await _viewModel.InitializeAsync();
        }
    }
}
