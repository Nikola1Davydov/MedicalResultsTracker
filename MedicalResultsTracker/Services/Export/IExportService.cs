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

        /// <summary>Полный локальный бэкап в JSON — чтобы перенести историю на другое устройство.</summary>
        Task<string> ExportBackupAsync();

        /// <summary>Восстановление из бэкапа. Возвращает количество добавленных анализов.</summary>
        Task<int> ImportBackupAsync(string filePath, bool replaceExisting = false);

        /// <summary>Открывает системный диалог "Поделиться" для готового файла.</summary>
        Task ShareAsync(string filePath, string title);
    }
}
