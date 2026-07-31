using MedicalResultsTracker.ViewModel;

namespace MedicalResultsTracker.View
{
    public partial class CatalogPage : ContentPage
    {
        private readonly CatalogViewModel _viewModel;

        public CatalogPage(CatalogViewModel viewModel)
        {
            InitializeComponent();

            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Возврат из редактора: список мог измениться.
            await _viewModel.InitializeAsync();
        }
    }
}
