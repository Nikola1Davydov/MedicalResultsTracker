namespace MedicalResultsTracker.Model
{
    /// <summary>Показатель из последнего анализа в сравнении с предыдущим.</summary>
    public class ParameterTrend
    {
        public required BloodParameter Current { get; init; }

        public BloodParameter? Previous { get; init; }

        public DateTime CurrentDate { get; init; }

        public DateTime? PreviousDate { get; init; }

        public string Name => Current.Name;

        public string? Unit => Current.Unit;

        public ParameterStatus Status => Current.Status;

        public double? Delta => Current.Value is double now && Previous?.Value is double before
            ? now - before
            : null;

        public double? DeltaPercent => Current.Value is double now && Previous?.Value is double before && before != 0
            ? (now - before) / Math.Abs(before) * 100d
            : null;

        public TrendDirection Direction => Delta switch
        {
            null => TrendDirection.Unknown,
            > 0 => TrendDirection.Up,
            < 0 => TrendDirection.Down,
            _ => TrendDirection.Flat
        };

        /// <summary>
        /// Стало лучше или хуже: сравниваем не сами значения, а расстояние до референсного диапазона.
        /// Если оба измерения в норме — считаем, что состояние стабильное.
        /// </summary>
        public TrendAssessment Assessment
        {
            get
            {
                if (Current.DistanceFromRange is not double now || Previous?.DistanceFromRange is not double before)
                {
                    return TrendAssessment.Unknown;
                }

                if (now == 0 && before == 0)
                {
                    return TrendAssessment.Stable;
                }

                const double epsilon = 1e-9;

                if (Math.Abs(now - before) <= epsilon)
                {
                    return TrendAssessment.Stable;
                }

                return now < before ? TrendAssessment.Improved : TrendAssessment.Worsened;
            }
        }
    }
}
