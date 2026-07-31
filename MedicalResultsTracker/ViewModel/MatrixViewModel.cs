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
        private readonly IBloodTestRepository _repository;
        private readonly IAnalysisService _analysis;

        [ObservableProperty]
        private bool _isEmpty = true;

        public MatrixViewModel(IBloodTestRepository repository, IAnalysisService analysis)
        {
            _repository = repository;
            _analysis = analysis;

            Title = S.Tab_Matrix;
        }

        /// <summary>Заголовки столбцов со значениями — даты сдач.</summary>
        public List<string> Dates { get; } = new();

        public List<MatrixRowViewModel> Rows { get; } = new();

        /// <summary>Сообщает странице, что таблицу нужно построить заново.</summary>
        public event EventHandler? Rebuilt;

        public override Task InitializeAsync() => RunAsync(LoadAsync, S.Err_Load);

        [RelayCommand]
        private Task Refresh() => RunAsync(LoadAsync, S.Err_Refresh);

        [RelayCommand]
        private Task OpenRow(MatrixRowViewModel? row) => row is null
            ? Task.CompletedTask
            : Shell.Current.GoToAsync(
                $"{AppRoutes.TrendDetail}?{AppRoutes.SeriesKeyParameter}={Uri.EscapeDataString(row.Key)}");

        private async Task LoadAsync()
        {
            Title = S.Tab_Matrix;

            IReadOnlyList<BloodTest> tests = await _repository.GetAllAsync();

            // Старые слева, новые справа: так таблица читается как история, а не задом наперёд.
            List<BloodTest> ordered = tests.OrderBy(t => t.Date).ToList();

            Dates.Clear();
            Rows.Clear();

            foreach (BloodTest test in ordered)
            {
                Dates.Add(test.Date.ToString("d", CultureInfo.CurrentCulture));
            }

            foreach (IGrouping<string, BloodParameter> group in ordered
                .SelectMany(t => t.Parameters)
                .GroupBy(_analysis.GetKey)
                .OrderBy(g => g.Last().Name, StringComparer.CurrentCulture))
            {
                BloodParameter newest = group.Last();

                MatrixRowViewModel row = new()
                {
                    Key = group.Key,
                    Name = newest.Name,
                    Unit = newest.Unit ?? string.Empty,
                    Range = newest.Range.IsDefined ? newest.Range.ToString() : S.Common_None,
                };

                foreach (BloodTest test in ordered)
                {
                    BloodParameter? cell = test.Parameters.FirstOrDefault(p => _analysis.GetKey(p) == group.Key);

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
