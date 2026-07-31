using System.Globalization;
using MedicalResultsTracker.Controls;
using MedicalResultsTracker.Model;
using MedicalResultsTracker.Resources.Strings;
using MedicalResultsTracker.Services.Analysis;
using MedicalResultsTracker.Services.Database;

namespace MedicalResultsTracker.ViewModel
{
    /// <summary>
    /// Сводная таблица: строка — показатель, столбцы — даты сдач, слева направо от старых к новым.
    /// Тот же вид, что и в выгрузке CSV: показатель, норма, единицы и дальше значения по датам.
    /// </summary>
    public partial class MatrixViewModel : BaseViewModel
    {
        private readonly IAnalysisService _analysis;
        private readonly IMatrixViewRepository _views;

        [ObservableProperty]
        private bool _isEmpty = true;

        [ObservableProperty]
        private bool _hasSelectedView;

        [ObservableProperty]
        private MatrixViewOption? _selectedView;

        public MatrixViewModel(IAnalysisService analysis, IMatrixViewRepository views)
        {
            _analysis = analysis;
            _views = views;

            Title = S.Tab_Matrix;
        }

        /// <summary>Заголовки столбцов со значениями — даты сдач.</summary>
        public List<string> Dates { get; } = new();

        public List<MatrixRowViewModel> Rows { get; } = new();

        /// <summary>«Все показатели» плюс наборы, собранные пользователем.</summary>
        public ObservableCollection<MatrixViewOption> Views { get; } = new();

        /// <summary>
        /// Набор, который нужно открыть при следующем построении таблицы.
        /// Ставится редактором сразу после сохранения: человек только что собрал набор — он и должен показаться.
        /// </summary>
        public Guid? PendingViewId { get; set; }

        /// <summary>Сообщает странице, что таблицу нужно построить заново.</summary>
        public event EventHandler? Rebuilt;

        public override Task InitializeAsync() => RunAsync(LoadAsync, S.Err_Load);

        [RelayCommand]
        private Task Refresh() => RunAsync(LoadAsync, S.Err_Refresh);

        partial void OnSelectedViewChanged(MatrixViewOption? value) => _ = RunAsync(LoadAsync, S.Err_Load);

        [RelayCommand]
        private Task NewView() => Shell.Current.GoToAsync(AppRoutes.ViewEdit);

        [RelayCommand]
        private Task EditView() => SelectedView?.Id is Guid id
            ? Shell.Current.GoToAsync($"{AppRoutes.ViewEdit}?{AppRoutes.ViewIdParameter}={id}")
            : Shell.Current.GoToAsync(AppRoutes.ViewEdit);

        [RelayCommand]
        private Task OpenRow(MatrixRowViewModel? row) => row is null
            ? Task.CompletedTask
            : Shell.Current.GoToAsync(
                $"{AppRoutes.TrendDetail}?{AppRoutes.SeriesKeyParameter}={Uri.EscapeDataString(row.Key)}");

        private async Task RefreshViewsAsync()
        {
            Guid? current = PendingViewId ?? SelectedView?.Id;
            PendingViewId = null;

            IReadOnlyList<MatrixView> saved = await _views.GetAllAsync();

            Views.Clear();
            Views.Add(new MatrixViewOption { Name = S.Matrix_AllValues });

            foreach (MatrixView view in saved)
            {
                Views.Add(new MatrixViewOption { Id = view.Id, Name = view.Name, Codes = view.Codes });
            }

            // Пересобранный список — новые объекты: возвращаем выбор по идентификатору.
            MatrixViewOption? restored = Views.FirstOrDefault(v => v.Id == current);

            if (!ReferenceEquals(restored, SelectedView))
            {
                SetProperty(ref _selectedView, restored ?? Views[0], nameof(SelectedView));
            }
        }

        private async Task LoadAsync()
        {
            Title = S.Tab_Matrix;

            await RefreshViewsAsync();

            // Столбцы уже сведены по датам: два бланка от одного числа — один столбец.
            ResultMatrix matrix = await _analysis.BuildMatrixAsync();

            // Набор задаёт и состав строк, и их порядок: пользователь выбирал показатели не случайно.
            List<string>? filter = SelectedView?.Codes;

            Dates.Clear();
            Rows.Clear();

            foreach (DateTime date in matrix.Dates)
            {
                Dates.Add(date.ToString("d", CultureInfo.CurrentCulture));
            }

            IEnumerable<MatrixLine> lines = filter is null
                ? matrix.Lines
                : matrix.Lines
                    .Where(line => filter.Contains(line.Key, StringComparer.OrdinalIgnoreCase))
                    .OrderBy(line => filter.FindIndex(c => string.Equals(c, line.Key, StringComparison.OrdinalIgnoreCase)));

            foreach (MatrixLine line in lines)
            {
                MatrixRowViewModel row = new()
                {
                    Key = line.Key,
                    Name = line.Newest.Name,
                    Unit = line.Newest.Unit ?? string.Empty,
                    Range = line.Newest.Range.IsDefined ? line.Newest.Range.ToString() : S.Common_None,
                };

                foreach (BloodParameter? cell in line.Cells)
                {
                    row.Cells.Add(new MatrixCellViewModel
                    {
                        Text = cell?.DisplayValue ?? S.Common_None,
                        Color = StatusPalette.For(cell?.Status ?? ParameterStatus.Unknown),
                        IsOutOfRange = cell?.Status is ParameterStatus.Low or ParameterStatus.High,
                    });
                }

                Rows.Add(row);
            }

            IsEmpty = Rows.Count == 0;
            HasSelectedView = SelectedView?.Id is not null;

            Rebuilt?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>Пункт списка наборов. Id пустой у пункта «все показатели».</summary>
    public sealed class MatrixViewOption
    {
        public Guid? Id { get; init; }

        public required string Name { get; init; }

        /// <summary>null — показывать всё; иначе состав набора в его порядке.</summary>
        public List<string>? Codes { get; init; }
    }

    /// <summary>Строка таблицы: один показатель со всеми его значениями по датам.</summary>
    public sealed class MatrixRowViewModel
    {
        public required string Key { get; init; }

        public required string Name { get; init; }

        public required string Unit { get; init; }

        public required string Range { get; init; }

        public List<MatrixCellViewModel> Cells { get; } = new();
    }

    /// <summary>Ячейка значения.</summary>
    public sealed class MatrixCellViewModel
    {
        public required string Text { get; init; }

        public required Color Color { get; init; }

        public required bool IsOutOfRange { get; init; }
    }
}
