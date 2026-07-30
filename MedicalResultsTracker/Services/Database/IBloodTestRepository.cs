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

        /// <summary>Вставляет или обновляет анализ вместе со строками показателей (в транзакции).</summary>
        Task SaveAsync(BloodTest test);

        Task DeleteAsync(Guid id);

        /// <summary>Полная очистка истории. Используется только по явному подтверждению в настройках.</summary>
        Task DeleteAllAsync();

        Task<int> CountAsync();
    }
}
