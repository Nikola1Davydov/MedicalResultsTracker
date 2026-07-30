using MedicalResultsTracker.ViewModel;

namespace MedicalResultsTracker.View
{
    public partial class TrendsPage : ContentPage
    {
        private readonly TrendsViewModel _viewModel;

        public TrendsPage(TrendsViewModel viewModel)
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
