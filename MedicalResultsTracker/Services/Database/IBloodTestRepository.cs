using MedicalResultsTracker.Model;

namespace MedicalResultsTracker.Services.Database
{
    public interface IBloodTestRepository
    {
        /// <summary>Все анализы, свежие сверху. Показатели загружены.</summary>
        Task<IReadOnlyList<BloodTest>> GetAllAsync();

        Task<BloodTest?> GetAsync(Guid id);

        /// <summary>Последний по дате анализ или null, если история пуста.</summary>
        Task<BloodTest?> GetLatestAsync();

        /// <summary>Анализ, предшествующий указанной дате — для сравнения "стало лучше/хуже".</summary>
        Task<BloodTest?> GetPreviousAsync(DateTime beforeDate);

        /// <summary>
        /// Уже сохранённый анализ за то же число, кроме <paramref name="exceptId"/>.
        /// Нужен, чтобы второй бланк от того же дня дописался в существующую запись,
        /// а не завёл рядом столбец с той же датой в шапке.
        /// </summary>
        Task<BloodTest?> GetByDateAsync(DateTime date, Guid exceptId);

        /// <summary>Вставляет или обновляет анализ вместе со строками показателей (в транзакции).</summary>
        Task SaveAsync(BloodTest test);

        Task DeleteAsync(Guid id);

        /// <summary>Полная очистка истории. Используется только по явному подтверждению в настройках.</summary>
        Task DeleteAllAsync();

        Task<int> CountAsync();

        /// <summary>Сколько измерений сохранено по каждому коду показателя. Нужно справочнику, чтобы
        /// не дать удалить используемую запись молча.</summary>
        Task<IReadOnlyDictionary<string, int>> GetUsageByCodeAsync();

        /// <summary>
        /// Переписывает код у всех измерений — так объединяются дубли справочника.
        /// Возвращает количество затронутых строк.
        /// </summary>
        Task<int> ReassignCodeAsync(string fromCode, string toCode);

        /// <summary>
        /// Записывает границы нормы во все измерения показателя. Нужно, когда норму
        /// не удалось вытащить из бланка и человек вписывает её сам: без неё приложение
        /// не может сказать, вышло значение за пределы или нет.
        /// </summary>
        Task<int> SetRangeAsync(string code, double? min, double? max);
    }
}
