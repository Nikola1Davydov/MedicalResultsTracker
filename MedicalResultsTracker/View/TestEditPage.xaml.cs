using MedicalResultsTracker.ViewModel;

namespace MedicalResultsTracker.View
{
    public partial class TestEditPage : ContentPage
    {
        private readonly TestEditViewModel _viewModel;

        private bool _initialized;

        public TestEditPage(TestEditViewModel viewModel)
        {
            InitializeComponent();

            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Загружаем один раз: иначе возврат из диалога затрёт несохранённые правки.
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            await _viewModel.InitializeAsync();
        }
    }
}
