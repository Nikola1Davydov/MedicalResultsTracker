using System.Globalization;
using MedicalResultsTracker.Controls;
using MedicalResultsTracker.Model;
using MedicalResultsTracker.Services.Analysis;

namespace MedicalResultsTracker.ViewModel
{
    /// <summary>Список показателей с мини-графиками динамики.</summary>
    public partial class TrendsViewModel : BaseViewModel
    {
        private readonly IAnalysisService _analysis;

        [ObservableProperty]
        private bool _isEmpty = true;

        [ObservableProperty]
        private bool _onlyWithHistory = true;

        public TrendsViewModel(IAnalysisService analysis)
        {
            _analysis = analysis;

            Title = "Динамика";
        }

        public ObservableCollection<SeriesItemViewModel> Series { get; } = new();

        public override Task InitializeAsync() => RunAsync(LoadAsync, "Не удалось построить графики");

        [RelayCommand]
        private Task Refresh() => RunAsync(LoadAsync, "Не удалось обновить графики");

        [RelayCommand]
        private Task Open(SeriesItemViewModel? item) => item is null
            ? Task.CompletedTask
            : Shell.Current.GoToAsync(
                $"{AppRoutes.TrendDetail}?{AppRoutes.SeriesKeyParameter}={Uri.EscapeDataString(item.Key)}");

        [RelayCommand]
        private Task ToggleFilter()
        {
            OnlyWithHistory = !OnlyWithHistory;

            return RunAsync(LoadAsync, "Не удалось обновить графики");
        }

        private async Task LoadAsync()
        {
            IReadOnlyList<ParameterSeries> series = await _analysis.GetSeriesAsync();

            Series.Clear();

            foreach (ParameterSeries item in series.Where(s => !OnlyWithHistory || s.HasTrend))
            {
                Series.Add(new SeriesItemViewModel(item));
            }

            IsEmpty = Series.Count == 0;
        }
    }

    /// <summary>Карточка показателя со спарклайном.</summary>
    public sealed class SeriesItemViewModel
    {
        public SeriesItemViewModel(ParameterSeries series)
        {
            Key = series.Key;
            Name = series.Name;
            Unit = series.Unit;
            PointCount = series.Points.Count;
            Chart = new TrendChartDrawable { Series = series, Compact = true };

            SeriesPoint? latest = series.Latest;

            LatestText = latest is null
                ? "—"
                : $"{latest.Value.ToString("0.####", CultureInfo.CurrentCulture)} {Unit}".Trim();

            StatusColor = StatusPalette.For(latest?.Status ?? ParameterStatus.Unknown);

            if (series.Points.Count >= 2)
            {
                double delta = series.Points[^1].Value - series.Points[^2].Value;

                DeltaText = delta switch
                {
                    > 0 => $"↑ {delta.ToString("0.####", CultureInfo.CurrentCulture)}",
                    < 0 => $"↓ {Math.Abs(delta).ToString("0.####", CultureInfo.CurrentCulture)}",
                    _ => "→ без изменений"
                };
            }
            else
            {
                DeltaText = "одно измерение";
            }
        }

        public string Key { get; }

        public string Name { get; }

        public string? Unit { get; }

        public int PointCount { get; }

        public string LatestText { get; }

        public string DeltaText { get; }

        public Color StatusColor { get; }

        public TrendChartDrawable Chart { get; }
    }
}
