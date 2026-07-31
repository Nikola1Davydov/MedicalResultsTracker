using MedicalResultsTracker.Services.Ai;
using MedicalResultsTracker.Services.Import;
using Xunit;

namespace MedicalResultsTracker.Tests
{
    /// <summary>
    /// Разбор ответа чат-бота целиком. Формат описан в запросе, но следуют ему по-разному:
    /// кто-то отдаёт markdown-таблицу, кто-то точки с запятой, кто-то добавляет шапку.
    /// </summary>
    public class TextImportTests
    {
        private static readonly TextImportService Import = new();

        [Fact]
        public void ReadsDateAndLaboratoryFromTheHeader()
        {
            AiDraft draft = Import.Parse("""
                Datum: 12.03.2026
                Labor: Labor Neuss
                Ferritin | 18 | ng/ml | 30 | 300
                """);

            Assert.Equal(new DateTime(2026, 3, 12), draft.Date);
            Assert.Equal("Labor Neuss", draft.Laboratory);
        }

        [Fact]
        public void ReadsMarkdownTableAndDropsItsHeader()
        {
            AiDraft draft = Import.Parse("""
                | Bezeichnung | Ergebnis | Einheit | Referenz |
                |---|---|---|---|
                | Ferritin | 18 | ng/ml | 30 – 300 |
                | Eisen | 45 | µg/dl | 33 – 193 |
                """);

            Assert.Equal(2, draft.Rows.Count);
            Assert.Equal("Ferritin", draft.Rows[0].Name);
            Assert.Equal(30, draft.Rows[0].RefMin);
            Assert.Equal(300, draft.Rows[0].RefMax);
        }

        [Theory]
        [InlineData("Ferritin;18;ng/ml")]
        [InlineData("Ferritin\t18\tng/ml")]
        [InlineData("Ferritin | 18 | ng/ml")]
        public void AcceptsEverySeparatorTheBotsUse(string line)
        {
            AiDraftRow row = Import.Parse(line).Rows.Single();

            Assert.Equal("Ferritin", row.Name);
            Assert.Equal(18, row.Value);
            Assert.Equal("ng/ml", row.Unit);
        }

        [Theory]
        // Односторонняя норма: заполняется только та граница, которая указана.
        [InlineData("bis 5,2", null, 5.2d)]
        [InlineData("< 5,2", null, 5.2d)]
        [InlineData("≤ 5,2", null, 5.2d)]
        [InlineData("ab 1,5", 1.5d, null)]
        [InlineData("> 1,5", 1.5d, null)]
        [InlineData("до 5,2", null, 5.2d)]
        [InlineData("от 1,5", 1.5d, null)]
        // Двусторонняя, разные виды тире.
        [InlineData("30 – 300", 30d, 300d)]
        [InlineData("30-300", 30d, 300d)]
        public void ReadsReferenceRangeGivenAsOneColumn(string printed, double? min, double? max)
        {
            AiDraftRow row = Import.Parse($"Ferritin | 18 | ng/ml | {printed}").Rows.Single();

            Assert.Equal(min, row.RefMin);
            Assert.Equal(max, row.RefMax);
        }

        [Fact]
        public void ReportsEmptyInputInsteadOfThrowing()
        {
            AiDraft draft = Import.Parse("   ");

            Assert.Empty(draft.Rows);
            Assert.NotEmpty(draft.Warnings);
        }

        [Fact]
        public void KeepsGoodRowsWhenOneLineIsUnreadable()
        {
            AiDraft draft = Import.Parse("""
                Ferritin | 18 | ng/ml
                это не строка таблицы
                Eisen | 45 | µg/dl
                """);

            Assert.Equal(2, draft.Rows.Count);
            Assert.NotEmpty(draft.Warnings);
        }

        /// <summary>
        /// Разбор не должен зависеть от языка интерфейса: ключевые слова бланка — часть данных,
        /// а не часть оформления. Однажды это уже ломалось автозаменой строк на ресурсы.
        /// </summary>
        [Fact]
        public void ReadsRussianHeaderFieldsToo()
        {
            AiDraft draft = Import.Parse("""
                Дата: 12.03.2026
                Лаборатория: Инвитро
                Ферритин | 18 | мкг/л | 30 | 300
                """);

            Assert.Equal(new DateTime(2026, 3, 12), draft.Date);
            Assert.Equal("Инвитро", draft.Laboratory);
            Assert.Single(draft.Rows);
        }
    }
}
