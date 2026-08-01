namespace MedicalResultsTracker.Services.UI
{
    /// <summary>
    /// Порог, выше которого измерение подсвечивается.
    ///
    /// Это **не** диагноз и не классификация гипертонии: приложение не имеет права
    /// ставить степени. Порог вписывает сам человек — со слов своего врача, — а приложение
    /// лишь сравнивает с ним число. Значения по умолчанию взяты как общеизвестный ориентир
    /// и подписаны в интерфейсе как настраиваемая цель, а не как норма.
    /// </summary>
    public static class BloodPressureTarget
    {
        public const int DefaultSystolic = 140;
        public const int DefaultDiastolic = 90;

        private const string SystolicKey = "pressure.target.systolic";
        private const string DiastolicKey = "pressure.target.diastolic";

        public static int Systolic
        {
            get => Preferences.Default.Get(SystolicKey, DefaultSystolic);
            set => Preferences.Default.Set(SystolicKey, value);
        }

        public static int Diastolic
        {
            get => Preferences.Default.Get(DiastolicKey, DefaultDiastolic);
            set => Preferences.Default.Set(DiastolicKey, value);
        }
    }
}
