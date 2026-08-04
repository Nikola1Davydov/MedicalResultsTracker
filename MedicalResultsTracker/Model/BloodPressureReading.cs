using SQLite;

namespace MedicalResultsTracker.Model
{
    /// <summary>
    /// Одно измерение давления.
    ///
    /// Отдельная сущность, а не строки в бланке анализа. У анализов один столбец на дату
    /// и второй бланк за то же число дописывается в первый — для давления это ровно наоборот:
    /// утром 130/85 и вечером 145/95 это два разных измерения, а не дубликат. Плюс верхнее
    /// и нижнее осмысленны только вместе, отдельными показателями их не разложить.
    /// </summary>
    [Table("blood_pressure")]
    public class BloodPressureReading
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Когда измерено — с точностью до минуты. Время хранится наравне с датой:
        /// утреннее и вечернее давление различаются, и без времени их не отличить.
        /// </summary>
        [Indexed]
        public DateTime MeasuredAt { get; set; } = DateTime.Now;

        /// <summary>Верхнее, систолическое.</summary>
        public int Systolic { get; set; }

        /// <summary>Нижнее, диастолическое.</summary>
        public int Diastolic { get; set; }

        /// <summary>Пульс. Не все тонометры его показывают, поэтому необязателен.</summary>
        public int? Pulse { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        public DateTime ModifiedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>«130/85» — то, как давление читают и записывают.</summary>
        [Ignore]
        public string Display => $"{Systolic}/{Diastolic}";

        /// <summary>
        /// Измерение выше порога, заданного пользователем в настройках.
        ///
        /// Намеренно не «гипертония» и не степень: приложение сравнивает с числом,
        /// которое человек сам вписал со слов врача, и ничего не диагностирует.
        /// </summary>
        public bool IsAbove(int systolicLimit, int diastolicLimit) =>
            Systolic > systolicLimit || Diastolic > diastolicLimit;

        /// <summary>
        /// Измерение ниже порога, заданного пользователем. Ноль означает «снизу не следить»:
        /// пороги нижней границы человек ставит не всегда, и без этой проверки любое
        /// измерение оказывалось бы «ниже нуля» никогда, а с нулём-порогом — всегда.
        /// </summary>
        public bool IsBelow(int systolicLimit, int diastolicLimit) =>
            (systolicLimit > 0 && Systolic < systolicLimit) ||
            (diastolicLimit > 0 && Diastolic < diastolicLimit);

        /// <summary>Значения, при которых запись не имеет смысла: опечатка или пустая форма.</summary>
        [Ignore]
        public bool IsPlausible =>
            Systolic is >= 40 and <= 300 &&
            Diastolic is >= 20 and <= 200 &&
            Diastolic < Systolic &&
            Pulse is null or (>= 20 and <= 250);
    }
}
