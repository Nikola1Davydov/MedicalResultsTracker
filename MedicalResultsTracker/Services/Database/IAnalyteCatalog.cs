using MedicalResultsTracker.Model;

namespace MedicalResultsTracker.Services.Database
{
    /// <summary>Справочник показателей: подсказывает название, единицы и типовую норму при ручном вводе.</summary>
    public interface IAnalyteCatalog
    {
        /// <summary>Заполняет каталог встроенным набором при первом запуске. Пользовательские записи не трогает.</summary>
        Task EnsureSeededAsync();

        /// <summary>Весь каталог, включая скрытые записи — для экрана справочника.</summary>
        Task<IReadOnlyList<Analyte>> GetAllAsync();

        Task<Analyte?> FindAsync(string code);

        /// <summary>
        /// Поиск по точному названию. Нужен, чтобы показатель, введённый руками, привязался
        /// к уже существующей записи каталога, а не завёл рядом вторую с тем же смыслом.
        /// </summary>
        Task<Analyte?> FindByNameAsync(string name);

        /// <summary>
        /// Поиск по части названия или кода — для подсказок в редакторе анализа.
        /// Скрытые записи не предлагаются.
        /// </summary>
        Task<IReadOnlyList<Analyte>> SearchAsync(string query, int limit = 20);

        Task SaveAsync(Analyte analyte);

        Task DeleteAsync(string code);
    }
}
