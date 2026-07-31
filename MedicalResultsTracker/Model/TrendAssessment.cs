namespace MedicalResultsTracker.Model
{
    /// <summary>Направление изменения самого числа.</summary>
    public enum TrendDirection
    {
        Unknown = 0,
        Flat = 1,
        Up = 2,
        Down = 3
    }

    /// <summary>
    /// Оценка изменения относительно нормы: рост показателя сам по себе не "хорошо" и не "плохо",
    /// значение имеет то, приблизилось ли оно к референсному диапазону.
    /// </summary>
    public enum TrendAssessment
    {
        Unknown = 0,
        Stable = 1,
        Improved = 2,
        Worsened = 3
    }
}
