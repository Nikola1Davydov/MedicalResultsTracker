using MedicalResultsTracker.Model;

namespace MedicalResultsTracker.Services.Database
{
    /// <summary>Измерения давления. Свежие сверху — их и смотрят чаще всего.</summary>
    public interface IBloodPressureRepository
    {
        Task<IReadOnlyList<BloodPressureReading>> GetAllAsync();

        Task<BloodPressureReading?> GetAsync(Guid id);

        /// <summary>Последнее измерение или null, если их ещё нет.</summary>
        Task<BloodPressureReading?> GetLatestAsync();

        Task SaveAsync(BloodPressureReading reading);

        Task DeleteAsync(Guid id);

        Task<int> CountAsync();
    }
}
