namespace MedicalResultsTracker.Model
{
    public enum ParameterStatus
    {
        /// <summary>Норма не задана или результат нечисловой — оценить нельзя.</summary>
        Unknown = 0,
        Normal = 1,
        Low = 2,
        High = 3
    }
}
