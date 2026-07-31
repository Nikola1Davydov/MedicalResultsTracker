using System.Globalization;
using MedicalResultsTracker.Resources.Strings;
using MedicalResultsTracker.Controls;
using MedicalResultsTracker.Model;

namespace MedicalResultsTracker.ViewModel
{
    /// <summary>Карточка показателя со спарклайном. Используется и на главном экране, и в динамике.</summary>
    public sealed class SeriesItemViewModel
    {
        public SeriesItemViewModel(ParameterSeries series, Analyte? analyte = null)
        {
            Key = series.Key;
            Name = series.Name;
            Unit = series.Unit;
            PointCount = series.Points.Count;
            Chart = new TrendChartDrawable { Series = series, Compact = true };

            IsFavorite = analyte?.IsFavorite ?? false;
            Category = AnalyteDisplay.Category(analyte?.Category);

            SeriesPoint? latest = series.Latest;

            LatestText = latest is null
                ? S.Common_None
                : $"{latest.Value.ToString("0.####", CultureInfo.CurrentCulture)} {Unit}".Trim();

            StatusColor = StatusPalette.For(latest?.Status ?? ParameterStatus.Unknown);

            if (series.Points.Count >= 2)
            {
                double delta = series.Points[^1].Value - series.Points[^2].Value;

                DeltaText = delta switch
                {
                    > 0 => $"↑ {delta.ToString("0.####", CultureInfo.CurrentCulture)}",
                    < 0 => $"↓ {Math.Abs(delta).ToString("0.####", CultureInfo.CurrentCulture)}",
                    _ => S.Trend_NoChange
                };
            }
            else
            {
                DeltaText = S.Trend_OneMeasurement;
            }

            LastDateText = latest is null ? string.Empty : latest.Date.ToString("d", CultureInfo.CurrentCulture);
        }

        public string Key { get; }

        public string Name { get; }

        public string? Unit { get; }

        public string Category { get; }

        public bool IsFavorite { get; }

        public int PointCount { get; }

        public string LatestText { get; }

        public string DeltaText { get; }

        public string LastDateText { get; }

        public Color StatusColor { get; }

        public TrendChartDrawable Chart { get; }
    }

    /// <summary>Группа показателей для сгруппированного списка. Наследник List — этого требует CollectionView.</summary>
    public sealed class SeriesGroupViewModel : List<SeriesItemViewModel>
    {
        public SeriesGroupViewModel(string name, IEnumerable<SeriesItemViewModel> items)
            : base(items)
        {
            Name = name;
        }

        public string Name { get; }

        public string Subtitle => Count == 1 ? S.Cat_OneParam : string.Format(S.Cat_ManyParams, Count);
    }
}
