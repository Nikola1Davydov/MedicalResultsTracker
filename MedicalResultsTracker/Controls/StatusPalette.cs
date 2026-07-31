using MedicalResultsTracker.Model;

namespace MedicalResultsTracker.Controls
{
    /// <summary>Единые цвета статусов для диаграмм и списков.</summary>
    public static class StatusPalette
    {
        public static readonly Color Normal = Color.FromArgb("#2E7D32");
        public static readonly Color Low = Color.FromArgb("#1565C0");
        public static readonly Color High = Color.FromArgb("#C62828");
        public static readonly Color Unknown = Color.FromArgb("#757575");

        public static readonly Color Improved = Color.FromArgb("#2E7D32");
        public static readonly Color Worsened = Color.FromArgb("#C62828");
        public static readonly Color Stable = Color.FromArgb("#757575");

        public static readonly Color RangeBand = Color.FromArgb("#332E7D32");
        public static readonly Color Line = Color.FromArgb("#455A64");
        public static readonly Color Axis = Color.FromArgb("#BDBDBD");

        public static Color For(ParameterStatus status) => status switch
        {
            ParameterStatus.Low => Low,
            ParameterStatus.High => High,
            ParameterStatus.Normal => Normal,
            _ => Unknown
        };

        public static Color For(TrendAssessment assessment) => assessment switch
        {
            TrendAssessment.Improved => Improved,
            TrendAssessment.Worsened => Worsened,
            TrendAssessment.Stable => Stable,
            _ => Unknown
        };

        public static string Describe(ParameterStatus status) => status switch
        {
            ParameterStatus.Low => "ниже нормы",
            ParameterStatus.High => "выше нормы",
            ParameterStatus.Normal => "в норме",
            _ => "норма не задана"
        };

        public static string Glyph(TrendDirection direction) => direction switch
        {
            TrendDirection.Up => "↑",
            TrendDirection.Down => "↓",
            TrendDirection.Flat => "→",
            _ => string.Empty
        };

        public static string Describe(TrendAssessment assessment) => assessment switch
        {
            TrendAssessment.Improved => "лучше, чем в прошлый раз",
            TrendAssessment.Worsened => "хуже, чем в прошлый раз",
            TrendAssessment.Stable => "без изменений",
            _ => "не с чем сравнить"
        };
    }
}
