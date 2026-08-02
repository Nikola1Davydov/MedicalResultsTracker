namespace MedicalResultsTracker.Services.Export
{
    public interface IExportService
    {
        /// <summary>
        /// Сводная таблица: строка — показатель, столбец — дата анализа.
        /// Это тот вид, в котором результаты обычно и хочется видеть рядом.
        /// </summary>
        Task<string> ExportMatrixCsvAsync();

        /// <summary>Плоская выгрузка: одна строка на каждое измерение. Удобна для сводных таблиц и импорта.</summary>
        Task<string> ExportFlatCsvAsync();

        /// <summary>
        /// Дневник давления отдельным файлом: дата, время, верхнее, нижнее, пульс, заметка.
        /// В сводную таблицу анализов он не помещается — там столбец на дату, а давление
        /// меряют по нескольку раз в день, и время в нём значимо.
        /// </summary>
        Task<string> ExportPressureCsvAsync();

        /// <summary>Полный локальный бэкап в JSON — чтобы перенести историю на другое устройство.</summary>
        Task<string> ExportBackupAsync();

        /// <summary>Восстановление из бэкапа. Возвращает количество добавленных записей — анализов и измерений давления.</summary>
        Task<int> ImportBackupAsync(string filePath, bool replaceExisting = false);

        /// <summary>
        /// Компактная markdown-таблица истории для передачи в любой ИИ-чат через системный
        /// «Поделиться» или буфер обмена. Персональных данных не содержит — только показатели,
        /// единицы, границы норм и даты.
        /// </summary>
        /// <param name="maxTests">Сколько последних анализов включать. 0 — все.</param>
        /// <param name="onlyKeys">
        /// Ключи показателей, которые нужно оставить, — то, что человек отобрал фильтрами
        /// на экране. null или пустой список — вся таблица. Дневник давления в отобранную
        /// выборку не попадает: спрашивают про конкретные показатели, а не про всё сразу.
        /// </param>
        Task<string> BuildTextSummaryAsync(int maxTests = 6, IReadOnlyCollection<string>? onlyKeys = null);

        /// <summary>Открывает системный диалог "Поделиться" для готового файла.</summary>
        Task ShareAsync(string filePath, string title);

        /// <summary>
        /// Открывает системный диалог «Поделиться» с текстом. Получателя выбирает пользователь —
        /// приложение не знает и не решает, в какое приложение уйдёт текст.
        /// </summary>
        Task ShareTextAsync(string text, string title);

        /// <summary>Кладёт текст в буфер обмена — запасной путь, если нужного приложения нет в списке «Поделиться».</summary>
        Task CopyToClipboardAsync(string text);
    }
}
