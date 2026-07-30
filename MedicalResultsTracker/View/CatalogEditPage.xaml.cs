using MedicalResultsTracker.ViewModel;

namespace MedicalResultsTracker.View
{
    public partial class CatalogEditPage : ContentPage
    {
        private readonly CatalogEditViewModel _viewModel;

        private bool _initialized;

        public CatalogEditPage(CatalogEditViewModel viewModel)
        {
            InitializeComponent();

            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Один раз: возврат из диалога не должен затирать несохранённые правки.
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            await _viewModel.InitializeAsync();
        }
    }
}
