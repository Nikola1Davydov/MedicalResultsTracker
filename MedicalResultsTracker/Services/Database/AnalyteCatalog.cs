using MedicalResultsTracker.Model;
using SQLite;

namespace MedicalResultsTracker.Services.Database
{
    /// <inheritdoc cref="IAnalyteCatalog"/>
    public sealed class AnalyteCatalog : IAnalyteCatalog
    {
        private readonly IMedicalDatabase _database;
        private readonly SemaphoreSlim _seedLock = new(1, 1);

        private bool _seeded;

        public AnalyteCatalog(IMedicalDatabase database)
        {
            _database = database;
        }

        public async Task EnsureSeededAsync()
        {
            if (_seeded)
            {
                return;
            }

            await _seedLock.WaitAsync().ConfigureAwait(false);

            try
            {
                if (_seeded)
                {
                    return;
                }

                SQLiteAsyncConnection connection = await _database.GetConnectionAsync().ConfigureAwait(false);

                List<Analyte> existing = await connection.Table<Analyte>().ToListAsync().ConfigureAwait(false);
                HashSet<string> existingCodes = existing.Select(a => a.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);

                // Добавляем только новые встроенные показатели, чтобы не затирать правки пользователя.
                List<Analyte> missing = AnalyteSeedData.BuiltIn
                    .Where(a => !existingCodes.Contains(a.Code))
                    .ToList();

                if (missing.Count > 0)
                {
                    await connection.InsertAllAsync(missing).ConfigureAwait(false);
                }

                _seeded = true;
            }
            finally
            {
                _seedLock.Release();
            }
        }

        public async Task<IReadOnlyList<Analyte>> GetAllAsync()
        {
            await EnsureSeededAsync().ConfigureAwait(false);

            SQLiteAsyncConnection connection = await _database.GetConnectionAsync().ConfigureAwait(false);

            List<Analyte> analytes = await connection.Table<Analyte>().ToListAsync().ConfigureAwait(false);

            return analytes
                .OrderBy(a => a.Category)
                .ThenBy(a => a.SortOrder)
                .ThenBy(a => a.Name)
                .ToList();
        }

        public async Task<Analyte?> FindAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            SQLiteAsyncConnection connection = await _database.GetConnectionAsync().ConfigureAwait(false);

            return await connection.Table<Analyte>()
                .Where(a => a.Code == code)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }

        public async Task<Analyte?> FindByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            IReadOnlyList<Analyte> all = await GetAllAsync().ConfigureAwait(false);
            string trimmed = name.Trim();

            return all.FirstOrDefault(a => string.Equals(a.Name.Trim(), trimmed, StringComparison.CurrentCultureIgnoreCase));
        }

        public async Task<IReadOnlyList<Analyte>> SearchAsync(string query, int limit = 20)
        {
            IReadOnlyList<Analyte> all = await GetAllAsync().ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(query))
            {
                return all.Take(limit).ToList();
            }

            string trimmed = query.Trim();

            return all
                .Where(a => a.Name.Contains(trimmed, StringComparison.CurrentCultureIgnoreCase)
                            || a.Code.Contains(trimmed, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(a => a.Name.StartsWith(trimmed, StringComparison.CurrentCultureIgnoreCase))
                .ThenBy(a => a.Name)
                .Take(limit)
                .ToList();
        }

        public async Task SaveAsync(Analyte analyte)
        {
            SQLiteAsyncConnection connection = await _database.GetConnectionAsync().ConfigureAwait(false);

            await connection.InsertOrReplaceAsync(analyte).ConfigureAwait(false);
        }

        public async Task DeleteAsync(string code)
        {
            SQLiteAsyncConnection connection = await _database.GetConnectionAsync().ConfigureAwait(false);

            await connection.ExecuteAsync("delete from analytes where Code = ?", code).ConfigureAwait(false);
        }
    }
}
