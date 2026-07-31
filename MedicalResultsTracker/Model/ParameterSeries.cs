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

        public bool HasTrend => Points.Count >= 2;
    }
}
