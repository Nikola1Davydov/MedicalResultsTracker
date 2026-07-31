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

        /// <summary>Выбранный набор до того, как в списке нажали пункт-действие.</summary>
        private Guid? _lastSelectedId;

        /// <summary>Сообщает странице, что таблицу нужно построить заново.</summary>
        public event EventHandler? Rebuilt;

        public override Task InitializeAsync() => RunAsync(LoadAsync, S.Err_Load);

        [RelayCommand]
        private Task Refresh() => RunAsync(LoadAsync, S.Err_Refresh);

        /// <summary>
        /// Выбор в списке. Два последних пункта — не наборы, а действия: завести новый набор
        /// и поправить выбранный. Кнопки рядом со списком отняли бы ширину у таблицы,
        /// а список и так открыт ровно тогда, когда человек думает про наборы.
        /// </summary>
        partial void OnSelectedViewChanged(MatrixViewOption? value)
        {
            if (value is null || value.Kind == MatrixViewKind.View)
            {
                _ = RunAsync(LoadAsync, S.Err_Load);
                return;
            }

            // Пункт-действие в списке не остаётся: возвращаем прежний выбор и уходим в редактор.
            MatrixViewOption? previous = Views.FirstOrDefault(v => v.Id == _lastSelectedId)
                ?? Views.FirstOrDefault(v => v.Kind == MatrixViewKind.View);

            SetProperty(ref _selectedView, previous, nameof(SelectedView));

            _ = value.Kind == MatrixViewKind.New
                ? Shell.Current.GoToAsync(AppRoutes.ViewEdit)
                : Shell.Current.GoToAsync($"{AppRoutes.ViewEdit}?{AppRoutes.ViewIdParameter}={_lastSelectedId}");
        }

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
            MatrixViewOption? restored = Views.FirstOrDefault(v => v.Id == current && v.Kind == MatrixViewKind.View);

            _lastSelectedId = restored?.Id;

            Views.Add(new MatrixViewOption { Name = S.Matrix_NewView, Kind = MatrixViewKind.New });

            if (restored?.Id is not null)
            {
                Views.Add(new MatrixViewOption { Name = S.Matrix_EditView, Kind = MatrixViewKind.Edit });
            }

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

            Rebuilt?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>Что означает пункт списка: выбор набора или действие над наборами.</summary>
    public enum MatrixViewKind
    {
        View,
        New,
        Edit,
    }

    /// <summary>Пункт списка наборов. Id пустой у пункта «все показатели» и у пунктов-действий.</summary>
    public sealed class MatrixViewOption
    {
        public Guid? Id { get; init; }

        public required string Name { get; init; }

        public MatrixViewKind Kind { get; init; } = MatrixViewKind.View;

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
