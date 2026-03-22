namespace MedicalResultsTracker.Model
{
    public class ReferenceRange
    {
        public double Min { get; set; }
        public double Max { get; set; }

        // Optional gender/age specific
        public string? Notes { get; set; }
    }
}
