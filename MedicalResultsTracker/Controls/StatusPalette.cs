using MedicalResultsTracker.Model;
using MedicalResultsTracker.Resources.Strings;

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
            ParameterStatus.Low => S.Csv_StatusLow,
            ParameterStatus.High => S.Csv_StatusHigh,
            ParameterStatus.Normal => S.Status_Normal,
            _ => S.Cat_NoRef
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
            TrendAssessment.Improved => S.Assess_Improved,
            TrendAssessment.Worsened => S.Assess_Worsened,
            TrendAssessment.Stable => S.Assess_Stable,
            _ => S.Assess_Unknown
        };
    }
}
