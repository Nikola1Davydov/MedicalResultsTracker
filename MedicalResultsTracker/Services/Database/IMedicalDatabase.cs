using SQLite;

namespace MedicalResultsTracker.Services.Database
{
    /// <summary>Доступ к локальному файлу базы. Никаких сетевых соединений здесь нет и не появится.</summary>
    public interface IMedicalDatabase
    {
        /// <summary>Полный путь к файлу базы — показываем его в настройках и используем при бэкапе.</summary>
        string DatabasePath { get; }

        /// <summary>Создаёт файл и таблицы при первом обращении. Безопасно вызывать многократно.</summary>
        Task<SQLiteAsyncConnection> GetConnectionAsync();
    }
}
