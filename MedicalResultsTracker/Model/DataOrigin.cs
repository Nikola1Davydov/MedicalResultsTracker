namespace MedicalResultsTracker.Model
{
    /// <summary>Откуда взялись данные. Нужно, чтобы отличать введённое руками от предложенного ассистентом.</summary>
    public enum DataOrigin
    {
        Manual = 0,

        /// <summary>Импорт из файла (CSV/бэкап), без внешних сервисов.</summary>
        Imported = 1,

        /// <summary>Черновик распознан ассистентом и подтверждён пользователем.</summary>
        AssistedReview = 2
    }
}
