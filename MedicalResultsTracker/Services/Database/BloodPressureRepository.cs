using MedicalResultsTracker.Model;
using SQLite;

namespace MedicalResultsTracker.Services.Database
{
    /// <inheritdoc cref="IBloodPressureRepository"/>
    public sealed class BloodPressureRepository : IBloodPressureRepository
    {
        private readonly IMedicalDatabase _database;

        public BloodPressureRepository(IMedicalDatabase database)
        {
            _database = database;
        }

        public async Task<IReadOnlyList<BloodPressureReading>> GetAllAsync()
        {
            SQLiteAsyncConnection connection = await _database.GetConnectionAsync().ConfigureAwait(false);

            return await connection.Table<BloodPressureReading>()
                .OrderByDescending(r => r.MeasuredAt)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<BloodPressureReading?> GetAsync(Guid id)
        {
            SQLiteAsyncConnection connection = await _database.GetConnectionAsync().ConfigureAwait(false);

            return await connection.Table<BloodPressureReading>()
                .Where(r => r.Id == id)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }

        public async Task<BloodPressureReading?> GetLatestAsync()
        {
            SQLiteAsyncConnection connection = await _database.GetConnectionAsync().ConfigureAwait(false);

            return await connection.Table<BloodPressureReading>()
                .OrderByDescending(r => r.MeasuredAt)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }

        public async Task SaveAsync(BloodPressureReading reading)
        {
            SQLiteAsyncConnection connection = await _database.GetConnectionAsync().ConfigureAwait(false);

            reading.ModifiedUtc = DateTime.UtcNow;

            await connection.InsertOrReplaceAsync(reading).ConfigureAwait(false);
        }

        public async Task DeleteAsync(Guid id)
        {
            SQLiteAsyncConnection connection = await _database.GetConnectionAsync().ConfigureAwait(false);

            await connection.ExecuteAsync("delete from blood_pressure where Id = ?", id).ConfigureAwait(false);
        }

        public async Task<int> CountAsync()
        {
            SQLiteAsyncConnection connection = await _database.GetConnectionAsync().ConfigureAwait(false);

            return await connection.Table<BloodPressureReading>().CountAsync().ConfigureAwait(false);
        }
    }
}
