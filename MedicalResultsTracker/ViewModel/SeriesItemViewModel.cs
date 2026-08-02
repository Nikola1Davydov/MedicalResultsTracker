using System.Globalization;
using System.Windows.Input;
using MedicalResultsTracker.Resources.Strings;
using MedicalResultsTracker.Controls;
using MedicalResultsTracker.Model;

namespace MedicalResultsTracker.ViewModel
{
    /// <summary>Карточка показателя со спарклайном на экране динамики.</summary>
    public sealed class SeriesItemViewModel
    {
        /// <summary>
        /// Команды жестов передаются в саму карточку намеренно: внутри <c>SwipeItem</c>
        /// привязка «найти родительскую ViewModel по дереву» работает ненадёжно —
        /// элемент жеста живёт в стороне от обычного дерева. Своё свойство привязывается всегда.
        /// </summary>
        public SeriesItemViewModel(
            ParameterSeries series,
            Analyte? analyte = null,
            ICommand? favoriteCommand = null,
            ICommand? hideCommand = null)
        {
            FavoriteCommand = favoriteCommand;
            HideCommand = hideCommand;

            Key = series.Key;
            Name = series.Name;
            Unit = series.Unit;
            PointCount = series.Points.Count;
            HasTrend = series.HasTrend;
            Chart = new TrendChartDrawable { Series = series, Compact = true };

            IsFavorite = analyte?.IsFavorite ?? false;
            IsHidden = analyte?.IsHidden ?? false;
            Category = AnalyteDisplay.Category(analyte?.Category);

            SeriesPoint? latest = series.Latest;

            Status = latest?.Status ?? ParameterStatus.Unknown;

            LatestText = latest is null
                ? S.Common_None
                : $"{latest.Value.ToString("0.####", CultureInfo.CurrentCulture)} {Unit}".Trim();

            StatusColor = StatusPalette.For(Status);

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

        /// <summary>Убран из списков через жест. Измерения остаются, показатель просто не мозолит глаза.</summary>
        public bool IsHidden { get; }

        /// <summary>Состояние последнего значения — по нему работают фильтры «выше» и «ниже нормы».</summary>
        public ParameterStatus Status { get; }

        /// <summary>Надпись на жесте вправо: она меняется вместе с тем, что жест сделает.</summary>
        public string FavoriteActionText => IsFavorite ? S.Swipe_Unfavorite : S.Swipe_Favorite;

        /// <summary>Жест вправо — избранное.</summary>
        public ICommand? FavoriteCommand { get; }

        /// <summary>Жест влево — убрать из списков.</summary>
        public ICommand? HideCommand { get; }

        public int PointCount { get; }

        /// <summary>Есть что сравнивать: две точки и более, все в одной шкале.</summary>
        public bool HasTrend { get; }

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
