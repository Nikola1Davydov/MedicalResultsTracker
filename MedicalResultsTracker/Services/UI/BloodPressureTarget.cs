namespace MedicalResultsTracker.Services.UI
{
    /// <summary>
    /// Пороги, за которыми измерение подсвечивается: сверху и снизу.
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

        /// <summary>
        /// Нижние пороги. Давление бывает не только высоким: низкое человек тоже отслеживает,
        /// а подсветить его было нечем. Ноль означает «снизу не следить» — это допустимый
        /// выбор, а не пустое поле.
        /// </summary>
        public const int DefaultSystolicLow = 90;
        public const int DefaultDiastolicLow = 60;

        private const string SystolicKey = "pressure.target.systolic";
        private const string DiastolicKey = "pressure.target.diastolic";
        private const string SystolicLowKey = "pressure.target.systolic.low";
        private const string DiastolicLowKey = "pressure.target.diastolic.low";

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

        public static int SystolicLow
        {
            get => Preferences.Default.Get(SystolicLowKey, DefaultSystolicLow);
            set => Preferences.Default.Set(SystolicLowKey, value);
        }

        public static int DiastolicLow
        {
            get => Preferences.Default.Get(DiastolicLowKey, DefaultDiastolicLow);
            set => Preferences.Default.Set(DiastolicLowKey, value);
        }
    }
}
