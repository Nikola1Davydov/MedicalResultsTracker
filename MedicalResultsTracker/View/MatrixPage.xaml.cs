using MedicalResultsTracker.Services.UI;

namespace MedicalResultsTracker.View
{
    /// <summary>
    /// Хост сводной таблицы. Сама таблица — компонент Blazor: разметка HTML держит
    /// колонку имён и строку дат на месте, а раскладку ячеек делает движок браузера.
    /// </summary>
    public partial class MatrixPage : ContentPage
    {
        private readonly MatrixState _state;

        public MatrixPage(MatrixState state)
        {
            InitializeComponent();

            _state = state;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Компонент живёт своей жизнью внутри WebView и о появлении страницы не знает.
            _state.RequestRefresh();
        }
    }
}
