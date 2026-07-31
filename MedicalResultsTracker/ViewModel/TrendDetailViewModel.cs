using System.Globalization;
using MedicalResultsTracker.Resources.Strings;
using MedicalResultsTracker.Controls;
using MedicalResultsTracker.Model;
using MedicalResultsTracker.Services.Analysis;

namespace MedicalResultsTracker.ViewModel
{
    /// <summary>Большой график одного показателя плюс таблица значений под ним.</summary>
    public partial class TrendDetailViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IAnalysisService _analysis;

        [ObservableProperty]
        private string _subtitle = string.Empty;

        [ObservableProperty]
        private string _rangeText = string.Empty;

        [ObservableProperty]
        private TrendChartDrawable _chart = new();

        [ObservableProperty]
        private bool _isEmpty = true;

        [ObservableProperty]
        private bool _hasData;

        private string? _key;

        public TrendDetailViewModel(IAnalysisService analysis)
        {
            _analysis = analysis;

            Title = S.Csv_Parameter;
        }

        public ObservableCollection<SeriesRowViewModel> Values { get; } = new();

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue(AppRoutes.SeriesKeyParameter, out object? value))
            {
                _key = Uri.UnescapeDataString(Convert.ToString(value) ?? string.Empty);
            }
        }

        public override Task InitializeAsync() => RunAsync(LoadAsync, S.Err_Chart);

        private async Task LoadAsync()
        {
            Values.Clear();

            if (string.IsNullOrEmpty(_key))
            {
                IsEmpty = true;
                HasData = false;
                return;
            }

            ParameterSeries? series = await _analysis.GetSeriesAsync(_key);

            if (series is null)
            {
                IsEmpty = true;
                Subtitle = S.Trend_NoData;
                return;
            }

            Title = series.Name;
            Chart = new TrendChartDrawable { Series = series };
            IsEmpty = false;
            HasData = true;

            ReferenceRange range = new() { Min = series.RefMin, Max = series.RefMax };

            RangeText = range.IsDefined
                ? string.Format(S.Trend_RefKnown, range, series.Unit).Trim()
                : S.Trend_RefUnknown;
            Subtitle = series.Points.Count == 1
                ? S.Trend_SinglePoint
                : string.Format(S.Trend_Since, series.Points.Count, series.Points[0].Date.ToString("dd.MM.yyyy"));

            // Свежие значения сверху — так удобнее сверяться с последним бланком.
            for (int i = series.Points.Count - 1; i >= 0; i--)
            {
                SeriesPoint point = series.Points[i];
                double? previous = i > 0 ? series.Points[i - 1].Value : null;

                Values.Add(new SeriesRowViewModel(point, previous, series.Unit));
            }
        }
    }

    /// <summary>Строка таблицы значений под графиком.</summary>
    public sealed class SeriesRowViewModel
    {
        public SeriesRowViewModel(SeriesPoint point, double? previous, string? unit)
        {
            DateText = point.Date.ToString("dd.MM.yyyy");
            ValueText = $"{point.Value.ToString("0.####", CultureInfo.CurrentCulture)} {unit}".Trim();
            StatusText = StatusPalette.Describe(point.Status);
            StatusColor = StatusPalette.For(point.Status);

            DeltaText = previous is double before
                ? (point.Value - before) switch
                {
                    > 0 => $"↑ {(point.Value - before).ToString("0.####", CultureInfo.CurrentCulture)}",
                    < 0 => $"↓ {Math.Abs(point.Value - before).ToString("0.####", CultureInfo.CurrentCulture)}",
                    _ => "→"
                }
                : string.Empty;
        }

        public string DateText { get; }

        public string ValueText { get; }

        public string DeltaText { get; }

        public string StatusText { get; }

        public Color StatusColor { get; }
    }
}
