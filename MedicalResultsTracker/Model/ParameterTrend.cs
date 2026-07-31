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

        /// <summary>
        /// Прошлое измерение сделано в той же шкале, что и нынешнее.
        ///
        /// Лаборатории меряют одно и то же по-разному, и 12 µg/l против 30 nmol/l — это не рост
        /// втрое, а другая единица. Пересчитать за пользователя приложение не имеет права:
        /// коэффициент зависит от показателя, и ошибка в нём дороже, чем отсутствие сравнения.
        /// Поэтому разные единицы означают «сравнить нельзя», а не «посчитаем как есть».
        /// </summary>
        public bool IsComparable => Previous is null || string.Equals(
            Current.Unit?.Trim(),
            Previous.Unit?.Trim(),
            StringComparison.CurrentCultureIgnoreCase);

        public double? Delta => IsComparable && Current.Value is double now && Previous?.Value is double before
            ? now - before
            : null;

        public double? DeltaPercent => IsComparable && Current.Value is double now && Previous?.Value is double before && before != 0
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
                if (!IsComparable ||
                    Current.DistanceFromRange is not double now ||
                    Previous?.DistanceFromRange is not double before)
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
