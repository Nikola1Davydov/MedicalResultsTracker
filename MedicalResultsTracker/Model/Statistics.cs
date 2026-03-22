namespace MedicalResultsTracker.Model
{
    internal class Statistics
    {
        public required string ShortName { get; set; }
        public required string Name { get; set; }
        public required string Unit { get; set; }
        public required object DefaultValue { get; set; }
        public object? Value { get; set; }
        public DateTime? Date { get; set; }
    }
}
