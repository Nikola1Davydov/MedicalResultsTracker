using MedicalResultsTracker.ViewModel;

namespace MedicalResultsTracker.View
{
    public partial class TrendDetailPage : ContentPage
    {
        private readonly TrendDetailViewModel _viewModel;

        public TrendDetailPage(TrendDetailViewModel viewModel)
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
