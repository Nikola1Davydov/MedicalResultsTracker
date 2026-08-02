using System.Text.Json.Serialization;
using SQLite;

namespace MedicalResultsTracker.Model
{
    /// <summary>Один бланк результатов (одна сдача анализа).</summary>
    [Table("blood_tests")]
    public class BloodTest
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Indexed]
        public DateTime Date { get; set; } = DateTime.Today;

        public string? Laboratory { get; set; }

        public string? Notes { get; set; }

        /// <summary>Путь к локальной копии исходного файла (фото/PDF), если пользователь его приложил.</summary>
        public string? SourceFilePath { get; set; }

        public DataOrigin Origin { get; set; } = DataOrigin.Manual;

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        public DateTime ModifiedUtc { get; set; } = DateTime.UtcNow;

        [Ignore]
        public List<BloodParameter> Parameters { get; set; } = new();

        [Ignore]
        [JsonIgnore]
        public int OutOfRangeCount => Parameters.Count(p => p.Status is ParameterStatus.Low or ParameterStatus.High);

        [Ignore]
        [JsonIgnore]
        public string Title => string.IsNullOrWhiteSpace(Laboratory)
            ? DateDisplay.Short(Date)
            : $"{DateDisplay.Short(Date)} · {Laboratory}";
    }
}
