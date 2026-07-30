using MedicalResultsTracker.Model;
using SQLite;

namespace MedicalResultsTracker.Services.Database
{
    /// <inheritdoc cref="IMedicalDatabase"/>
    public sealed class MedicalDatabase : IMedicalDatabase
    {
        public const string FileName = "medical-results.db3";

        private const SQLiteOpenFlags Flags =
            SQLiteOpenFlags.ReadWrite |
            SQLiteOpenFlags.Create |
            SQLiteOpenFlags.SharedCache |
            SQLiteOpenFlags.FullMutex;

        private readonly SemaphoreSlim _initLock = new(1, 1);

        private SQLiteAsyncConnection? _connection;

        public MedicalDatabase()
        {
            DatabasePath = Path.Combine(FileSystem.AppDataDirectory, FileName);
        }

        public string DatabasePath { get; }

        public async Task<SQLiteAsyncConnection> GetConnectionAsync()
        {
            if (_connection is not null)
            {
                return _connection;
            }

            await _initLock.WaitAsync().ConfigureAwait(false);

            try
            {
                if (_connection is null)
                {
                    SQLiteAsyncConnection connection = new(DatabasePath, Flags, storeDateTimeAsTicks: true);

                    await connection.CreateTableAsync<BloodTest>().ConfigureAwait(false);
                    await connection.CreateTableAsync<BloodParameter>().ConfigureAwait(false);
                    await connection.CreateTableAsync<Analyte>().ConfigureAwait(false);

                    _connection = connection;
                }
            }
            finally
            {
                _initLock.Release();
            }

            return _connection;
        }
    }
}
