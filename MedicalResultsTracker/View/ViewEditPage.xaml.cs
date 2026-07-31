using MedicalResultsTracker.ViewModel;

namespace MedicalResultsTracker.View
{
    public partial class ViewEditPage : ContentPage
    {
        private readonly ViewEditViewModel _viewModel;

        private bool _initialized;

        public ViewEditPage(ViewEditViewModel viewModel)
        {
            InitializeComponent();

            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Один раз: возврат из диалога не должен сбрасывать расставленные галочки.
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            await _viewModel.InitializeAsync();
        }
    }
}
