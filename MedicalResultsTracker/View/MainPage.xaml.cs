using MedicalResultsTracker.ViewModel;

namespace MedicalResultsTracker.View
{
    public partial class MainPage : ContentPage
    {
        private readonly MainViewModel _viewModel;

        public MainPage(MainViewModel viewModel)
        {
            InitializeComponent();

            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Данные могли измениться на другом экране — перечитываем при каждом показе.
            await _viewModel.InitializeAsync();
        }
    }
}
