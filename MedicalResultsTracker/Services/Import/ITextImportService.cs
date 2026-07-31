using MedicalResultsTracker.Model;
using MedicalResultsTracker.Services.Ai;

namespace MedicalResultsTracker.Services.Import
{
    /// <summary>
    /// Разбор таблицы результатов, вставленной текстом.
    /// Смысл: распознаванием бланка занимается чат-бот пользователя, а приложение
    /// принимает готовый текст. Ни ключей, ни сети на нашей стороне при этом не нужно.
    /// </summary>
    public interface ITextImportService
    {
        /// <summary>
        /// Запрос для чат-бота: описывает формат, в котором приложение ждёт данные.
        ///
        /// К запросу прикладывается список уже известных названий с единицами. Лаборатории
        /// пишут один и тот же показатель по-разному — «Hb», «Hemoglobin», «Hämoglobin», —
        /// и без списка каждая такая запись завела бы в таблице соседнюю строку с тем же смыслом.
        /// Значения в список не идут: чат-боту нужны только названия и единицы.
        /// </summary>
        string BuildPrompt(IReadOnlyList<Analyte> known);

        /// <summary>
        /// Разбирает вставленный текст в черновик. Никогда не бросает исключение на кривом вводе:
        /// что не разобралось, попадает в <see cref="AiDraft.Warnings"/>.
        /// </summary>
        AiDraft Parse(string text);
    }
}
