namespace MedicalResultsTracker.Model
{
    public class BloodParameter
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime? Date { get; set; }
        public required string Name { get; set; }
        public string? Unit { get; set; }
        public object? DefaultValue { get; set; }
        public object? Value { get; set; }
    }
}
