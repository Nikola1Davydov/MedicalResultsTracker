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
        /// <summary>Запрос для чат-бота: описывает формат, в котором приложение ждёт данные.</summary>
        string PromptForChat { get; }

        /// <summary>
        /// Разбирает вставленный текст в черновик. Никогда не бросает исключение на кривом вводе:
        /// что не разобралось, попадает в <see cref="AiDraft.Warnings"/>.
        /// </summary>
        AiDraft Parse(string text);
    }
}
