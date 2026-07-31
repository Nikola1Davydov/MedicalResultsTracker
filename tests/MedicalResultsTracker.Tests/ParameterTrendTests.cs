using MedicalResultsTracker.Model;
using Xunit;

namespace MedicalResultsTracker.Tests
{
    /// <summary>
    /// Оценка «лучше или хуже». Рост числа сам по себе ничего не значит: для ферритина
    /// вверх обычно хорошо, для холестерина — обычно плохо. Сравнивается расстояние
    /// до нормы, и вот это правило проверять нужно.
    /// </summary>
    public class ParameterTrendTests
    {
        private static BloodParameter Measure(double? value, double? min, double? max, string? unit = null) => new()
        {
            Name = "Ferritin",
            Value = value,
            RefMin = min,
            RefMax = max,
            Unit = unit,
        };

        [Theory]
        [InlineData(50, 30d, 300d, ParameterStatus.Normal)]
        [InlineData(18, 30d, 300d, ParameterStatus.Low)]
        [InlineData(400, 30d, 300d, ParameterStatus.High)]
        // Границы включительно: ровно на краю — ещё норма.
        [InlineData(30, 30d, 300d, ParameterStatus.Normal)]
        [InlineData(300, 30d, 300d, ParameterStatus.Normal)]
        // Односторонняя норма проверяет только свою сторону.
        [InlineData(400, null, 300d, ParameterStatus.High)]
        [InlineData(400, 30d, null, ParameterStatus.Normal)]
        public void JudgesOnlyAgainstTheRangeFromTheForm(
            double value, double? min, double? max, ParameterStatus expected) =>
            Assert.Equal(expected, Measure(value, min, max).Status);

        [Fact]
        public void SaysNothingWithoutAReferenceRange() =>
            Assert.Equal(ParameterStatus.Unknown, Measure(50, null, null).Status);

        [Fact]
        public void SaysNothingAboutNonNumericResults() =>
            Assert.Equal(ParameterStatus.Unknown, Measure(null, 30, 300).Status);

        [Theory]
        // Внутри нормы расстояние нулевое, каким бы ни было само число.
        [InlineData(50, 30d, 300d, 0d)]
        [InlineData(18, 30d, 300d, 12d)]
        [InlineData(400, 30d, 300d, 100d)]
        public void MeasuresHowFarOutsideTheRangeItIs(
            double value, double? min, double? max, double expected) =>
            Assert.Equal(expected, Measure(value, min, max).DistanceFromRange);

        [Fact]
        public void HasNoDistanceWithoutARange() =>
            Assert.Null(Measure(50, null, null).DistanceFromRange);

        [Theory]
        // Ближе к норме — лучше, дальше — хуже, оба внутри — без изменений.
        [InlineData(18, 25, TrendAssessment.Improved)]
        [InlineData(25, 18, TrendAssessment.Worsened)]
        [InlineData(50, 60, TrendAssessment.Stable)]
        // Выход за норму — ухудшение, даже если раньше был край диапазона.
        [InlineData(30, 20, TrendAssessment.Worsened)]
        public void ComparesByDistanceToTheRangeNotByTheNumber(
            double before, double now, TrendAssessment expected)
        {
            ParameterTrend trend = new()
            {
                Current = Measure(now, 30, 300),
                Previous = Measure(before, 30, 300),
            };

            Assert.Equal(expected, trend.Assessment);
        }

        /// <summary>
        /// 12 µg/l против 30 nmol/l — это не рост втрое, а другая единица. Приложение
        /// не имеет права ни пересчитывать, ни делать вид, что числа сравнимы.
        /// </summary>
        [Fact]
        public void RefusesToCompareAcrossDifferentUnits()
        {
            ParameterTrend trend = new()
            {
                Current = Measure(30, 30d, 100d, "nmol/l"),
                Previous = Measure(12, 12d, 40d, "µg/l"),
            };

            Assert.False(trend.IsComparable);
            Assert.Equal(TrendAssessment.Unknown, trend.Assessment);
            Assert.Null(trend.Delta);
            Assert.Null(trend.DeltaPercent);
        }

        [Theory]
        [InlineData("ng/ml", "ng/ml")]
        [InlineData("ng/ml", "NG/ML")]
        [InlineData("ng/ml", " ng/ml ")]
        [InlineData(null, null)]
        public void ComparesWhenTheUnitIsTheSame(string? before, string? now)
        {
            ParameterTrend trend = new()
            {
                Current = Measure(50, 30d, 300d, now),
                Previous = Measure(18, 30d, 300d, before),
            };

            Assert.True(trend.IsComparable);
            Assert.Equal(TrendAssessment.Improved, trend.Assessment);
        }

        [Fact]
        public void SaysNothingWithoutAPreviousMeasurement()
        {
            ParameterTrend trend = new() { Current = Measure(50, 30, 300) };

            Assert.Equal(TrendAssessment.Unknown, trend.Assessment);
        }
    }
}
