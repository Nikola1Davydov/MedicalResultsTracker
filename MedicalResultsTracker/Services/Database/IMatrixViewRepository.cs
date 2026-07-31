using MedicalResultsTracker.Model;

namespace MedicalResultsTracker.Services.Database
{
    /// <summary>Наборы показателей для таблицы, собранные пользователем.</summary>
    public interface IMatrixViewRepository
    {
        /// <summary>Все наборы вместе с их составом.</summary>
        Task<IReadOnlyList<MatrixView>> GetAllAsync();

        Task<MatrixView?> GetAsync(Guid id);

        /// <summary>Сохраняет набор и его состав целиком: состав всегда переписывается.</summary>
        Task SaveAsync(MatrixView view);

        Task DeleteAsync(Guid id);
    }
}
