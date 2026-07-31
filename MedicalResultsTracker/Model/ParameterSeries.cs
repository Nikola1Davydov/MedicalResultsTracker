namespace MedicalResultsTracker.Model
{
    /// <summary>Одна точка на графике показателя.</summary>
    public class SeriesPoint
    {
        public DateTime Date { get; init; }

        public double Value { get; init; }

        public ParameterStatus Status { get; init; }

        public double? RefMin { get; init; }

        public double? RefMax { get; init; }

        /// <summary>
        /// Единица этого измерения. Хранится у точки, а не у ряда: лаборатории меряют
        /// одно и то же по-разному, и без этого 12 µg/l и 30 nmol/l легли бы на одну линию.
        /// </summary>
        public string? Unit { get; init; }
    }

    /// <summary>История одного показателя по всем анализам — источник данных для диаграммы.</summary>
    public class ParameterSeries
    {
        public required string Key { get; init; }

        public required string Name { get; init; }

        public string? Unit { get; init; }

        public IReadOnlyList<SeriesPoint> Points { get; init; } = Array.Empty<SeriesPoint>();

        /// <summary>Норма из самого свежего измерения — её и рисуем полосой на графике.</summary>
        public double? RefMin => Points.LastOrDefault()?.RefMin;

        public double? RefMax => Points.LastOrDefault()?.RefMax;

        public SeriesPoint? Latest => Points.Count > 0 ? Points[^1] : null;

        /// <summary>
        /// Измерения сделаны в разных единицах.
        ///
        /// Сравнивать такие значения нельзя: на графике вышел бы скачок, которого не было,
        /// а «стало лучше» посчиталось бы по числам из разных шкал. Пересчитать за пользователя
        /// приложение не имеет права — переводной коэффициент зависит от показателя, и ошибка
        /// в нём стоит дороже, чем отсутствие графика. Поэтому такой ряд честно помечается.
        /// </summary>
        public bool HasMixedUnits => Points
            .Select(p => p.Unit?.Trim())
            .Where(u => !string.IsNullOrEmpty(u))
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .Count() > 1;

        /// <summary>Ряд годится для сравнения, только если все точки в одной шкале.</summary>
        public bool HasTrend => Points.Count >= 2 && !HasMixedUnits;
    }
}
