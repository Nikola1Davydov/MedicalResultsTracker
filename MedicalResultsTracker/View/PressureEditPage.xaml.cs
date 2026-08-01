using MedicalResultsTracker.ViewModel;

namespace MedicalResultsTracker.View
{
    public partial class PressureEditPage : ContentPage
    {
        private readonly PressureEditViewModel _viewModel;

        public PressureEditPage(PressureEditViewModel viewModel)
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
