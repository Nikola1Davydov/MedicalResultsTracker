using System.Globalization;
using MedicalResultsTracker.Controls;
using MedicalResultsTracker.Model;

namespace MedicalResultsTracker.ViewModel
{
    /// <summary>Строка "показатель + как изменился" для списков. Пересоздаётся при обновлении данных.</summary>
    public sealed class TrendItemViewModel
    {
        private readonly ParameterTrend _trend;

        /// <param name="key">Ключ показателя из <see cref="Services.Analysis.IAnalysisService.GetKey"/> — по нему открывается график.</param>
        public TrendItemViewModel(ParameterTrend trend, string key)
        {
            _trend = trend;
            Key = key;
        }

        public string Key { get; }

        public string Name => _trend.Name;

        public string Value => string.IsNullOrEmpty(_trend.Current.Unit)
            ? _trend.Current.DisplayValue
            : $"{_trend.Current.DisplayValue} {_trend.Current.Unit}";

        public string RangeText => _trend.Current.Range.IsDefined
            ? $"норма {_trend.Current.Range}"
            : "норма не указана";

        public string StatusText => StatusPalette.Describe(_trend.Status);

        public Color StatusColor => StatusPalette.For(_trend.Status);

        public bool IsOutOfRange => _trend.Status is ParameterStatus.Low or ParameterStatus.High;

        public string DeltaText
        {
            get
            {
                if (_trend.Delta is not double delta)
                {
                    return string.Empty;
                }

                string glyph = StatusPalette.Glyph(_trend.Direction);
                string value = Math.Abs(delta).ToString("0.####", CultureInfo.CurrentCulture);
                string percent = _trend.DeltaPercent is double p
                    ? $" ({p:+0.#;-0.#;0}%)"
                    : string.Empty;

                return $"{glyph} {value}{percent}";
            }
        }

        public string AssessmentText => StatusPalette.Describe(_trend.Assessment);

        public Color AssessmentColor => StatusPalette.For(_trend.Assessment);

        public bool HasComparison => _trend.Previous is not null;

        public string ComparisonHint => _trend.PreviousDate is DateTime date
            ? $"было {_trend.Previous?.DisplayValue} · {date:dd.MM.yyyy}"
            : "первое измерение";
    }
}
