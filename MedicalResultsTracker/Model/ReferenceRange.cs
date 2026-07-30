using System.Globalization;

namespace MedicalResultsTracker.Model
{
    /// <summary>Референсный диапазон. Обе границы необязательны: бывает "до 5.2" или "от 30".</summary>
    public class ReferenceRange
    {
        public double? Min { get; set; }

        public double? Max { get; set; }

        // Optional gender/age specific
        public string? Notes { get; set; }

        public bool IsDefined => Min is not null || Max is not null;

        public bool Contains(double value) =>
            (Min is not double min || value >= min) &&
            (Max is not double max || value <= max);

        public override string ToString()
        {
            CultureInfo culture = CultureInfo.CurrentCulture;

            return (Min, Max) switch
            {
                (double min, double max) => $"{min.ToString("0.####", culture)} – {max.ToString("0.####", culture)}",
                (double min, null) => $"≥ {min.ToString("0.####", culture)}",
                (null, double max) => $"≤ {max.ToString("0.####", culture)}",
                _ => string.Empty
            };
        }
    }
}
