namespace MedicalResultsTracker.Model
{
    public class BloodTest
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime Date { get; set; }

        public string? Laboratory { get; set; }

        public string? SourceFilePath { get; set; }

        public List<BloodParameter> Parameters { get; set; } = new();

    }
}
