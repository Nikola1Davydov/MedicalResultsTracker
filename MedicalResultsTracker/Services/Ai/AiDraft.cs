using MedicalResultsTracker.Model;

namespace MedicalResultsTracker.Services.Ai
{
    /// <summary>
    /// Результат распознавания бланка — именно черновик, а не готовая запись.
    /// В базу он попадает только после того, как пользователь просмотрел и подтвердил каждую строку.
    /// </summary>
    public sealed class AiDraft
    {
        public DateTime? Date { get; set; }

        public string? Laboratory { get; set; }

        public List<AiDraftRow> Rows { get; set; } = new();

        /// <summary>Что ассистент не смог разобрать — показываем пользователю, чтобы он дозаполнил вручную.</summary>
        public List<string> Warnings { get; set; } = new();

        public BloodTest ToBloodTest()
        {
            BloodTest test = new()
            {
                Date = Date ?? DateTime.Today,
                Laboratory = Laboratory,
                Origin = DataOrigin.AssistedReview,
            };

            test.Parameters = Rows.Select((row, index) => new BloodParameter
            {
                TestId = test.Id,
                Code = row.Code,
                Name = row.Name,
                Unit = row.Unit,
                Value = row.Value,
                TextValue = row.TextValue,
                RefMin = row.RefMin,
                RefMax = row.RefMax,
                SortOrder = index,
            }).ToList();

            return test;
        }
    }

    public sealed class AiDraftRow
    {
        public string? Code { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Unit { get; set; }

        public double? Value { get; set; }

        public string? TextValue { get; set; }

        public double? RefMin { get; set; }

        public double? RefMax { get; set; }

        /// <summary>Уверенность распознавания 0..1. Строки с низкой уверенностью подсвечиваем в UI.</summary>
        public double Confidence { get; set; } = 1d;
    }
}
