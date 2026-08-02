using System.Globalization;

namespace MedicalResultsTracker.Model
{
    /// <summary>
    /// Даты на экране. Формат берётся у языка интерфейса, а не пишется в коде: «02.08.2026»
    /// правильно для немецкого и русского, но не для английского, а язык человек выбирает
    /// в настройках. Захардкоженный формат ломается ровно там, где его никто не проверяет.
    /// </summary>
    public static class DateDisplay
    {
        /// <summary>Дата в привычном для языка виде: «02.08.2026», «8/2/2026».</summary>
        public static string Short(DateTime date) => date.ToString("d", CultureInfo.CurrentCulture);

        /// <summary>
        /// Компактная дата для подписей на графике: там помещается несколько символов,
        /// и четырёхзначный год их отнимает. Порядок частей остаётся тот же, что у языка.
        /// </summary>
        public static string Compact(DateTime date) =>
            date.ToString(CompactPattern, CultureInfo.CurrentCulture);

        /// <summary>
        /// Короткий формат языка с двузначным годом. Пересчитывается при каждом обращении:
        /// язык меняется на ходу, из настроек, без перезапуска.
        /// </summary>
        private static string CompactPattern =>
            CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern
                .Replace("yyyy", "yy")
                .Replace("YYYY", "yy");
    }
}
