using MedicalResultsTracker.Services.Ai;
using MedicalResultsTracker.Services.Import;
using Xunit;

namespace MedicalResultsTracker.Tests
{
    /// <summary>
    /// Разбор чисел из бланка. Самое опасное место приложения: ошибка здесь не падает
    /// и ничего не ломает — она молча кладёт неверное число в историю болезни.
    /// </summary>
    public class NumberParsingTests
    {
        private static readonly TextImportService Import = new();

        private static double? ValueOf(string printed) =>
            Import.Parse($"Test | {printed} | mg/dl").Rows.Single().Value;

        [Theory]
        // Немецкая запись: точка разделяет разряды, запятая — дробную часть.
        [InlineData("254.000", 254000)]
        [InlineData("1.234", 1234)]
        [InlineData("1.234.567", 1234567)]
        [InlineData("12,7", 12.7)]
        [InlineData("1.234,5", 1234.5)]
        // Английская запись: наоборот.
        [InlineData("1,234.5", 1234.5)]
        [InlineData("1,234,567", 1234567)]
        // Пробелы как разделитель разрядов, в том числе неразрывные.
        [InlineData("1 234,5", 1234.5)]
        [InlineData("1 234,5", 1234.5)]
        [InlineData("1 234,5", 1234.5)]
        // Швейцарский апостроф.
        [InlineData("1'234.5", 1234.5)]
        // Обычные дроби разбираются как дроби.
        [InlineData("4.5", 4.5)]
        [InlineData("0.123", 0.123)]
        [InlineData("0,123", 0.123)]
        [InlineData("116.0", 116)]
        [InlineData("13.25", 13.25)]
        // Четыре цифры справа разрядом быть не могут.
        [InlineData("1.2345", 1.2345)]
        // Целых больше трёх — группировка невозможна.
        [InlineData("1234.567", 1234.567)]
        public void ReadsNumberAsPrintedOnTheForm(string printed, double expected) =>
            Assert.Equal(expected, ValueOf(printed).GetValueOrDefault(), precision: 6);

        [Fact]
        public void KeepsNonNumericResultAsText()
        {
            AiDraftRow row = Import.Parse("Borrelien IgG | negativ | -").Rows.Single();

            Assert.Null(row.Value);
            Assert.Equal("negativ", row.TextValue);
        }

        /// <summary>
        /// «254.000» читается как 254 000, но однозначной эту запись не назовёшь.
        /// Приложение обязано сказать об этом до сохранения, а не решать за человека молча.
        /// </summary>
        [Theory]
        [InlineData("254.000")]
        [InlineData("1.234")]
        public void WarnsWhenGroupingIsIndistinguishableFromDecimals(string printed)
        {
            AiDraft draft = Import.Parse($"Thrombozyten | {printed} | /nl");

            Assert.NotEmpty(draft.Warnings);
        }

        [Theory]
        [InlineData("4,5")]
        [InlineData("0.123")]
        [InlineData("116.0")]
        [InlineData("1.234,5")]
        public void StaysQuietWhenTheNotationIsUnambiguous(string printed)
        {
            AiDraft draft = Import.Parse($"Kalium | {printed} | mmol/l");

            Assert.Empty(draft.Warnings);
        }
    }
}
