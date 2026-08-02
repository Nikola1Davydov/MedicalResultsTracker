using MedicalResultsTracker.Model;
using SQLite;

namespace MedicalResultsTracker.Services.Database
{
    /// <inheritdoc cref="IAnalyteCatalog"/>
    public sealed class AnalyteCatalog : IAnalyteCatalog
    {
        private const string VersionKey = "catalog.seed.version";

        private readonly IMedicalDatabase _database;
        private readonly SemaphoreSlim _seedLock = new(1, 1);

        private bool _seeded;

        /// <summary>
        /// Каталог читается на каждое нажатие клавиши в поиске показателя, а таблица маленькая
        /// и меняется редко — держим её в памяти и сбрасываем при любой записи.
        /// </summary>
        private IReadOnlyList<Analyte>? _cache;

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

                Dictionary<string, Analyte> byCode = existing
                    .GroupBy(a => a.Code, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                int storedVersion = Preferences.Default.Get(VersionKey, 0);
                bool refresh = storedVersion < AnalyteSeedData.Version;

                List<Analyte> toWrite = new();

                foreach (Analyte seed in AnalyteSeedData.BuiltIn)
                {
                    if (!byCode.TryGetValue(seed.Code, out Analyte? current))
                    {
                        toWrite.Add(seed);
                        continue;
                    }

                    // Запись, которую человек правил сам, не трогаем никогда: там стоят
                    // границы его лаборатории, а не типовые, и вернуть их к типовым молча —
                    // это подменить данные в медицинской записи.
                    if (!refresh || current.IsCustomized)
                    {
                        continue;
                    }

                    // Обновление набора: названия, единицы и типовые нормы берём новые,
                    // а выбор пользователя — избранное и скрытые — сохраняем.
                    current.Name = seed.Name;
                    current.Unit = seed.Unit;
                    current.Category = seed.Category;
                    current.RefMin = seed.RefMin;
                    current.RefMax = seed.RefMax;
                    current.Notes = seed.Notes;
                    current.SortOrder = seed.SortOrder;
                    current.IsBuiltIn = true;

                    toWrite.Add(current);
                }

                foreach (Analyte analyte in toWrite)
                {
                    await connection.InsertOrReplaceAsync(analyte).ConfigureAwait(false);
                }

                if (toWrite.Count > 0)
                {
                    _cache = null;
                }

                Preferences.Default.Set(VersionKey, AnalyteSeedData.Version);

                _seeded = true;
            }
            finally
            {
                _seedLock.Release();
            }
        }

        public async Task<IReadOnlyList<Analyte>> GetAllAsync()
        {
            if (_cache is not null)
            {
                return _cache;
            }

            await EnsureSeededAsync().ConfigureAwait(false);

            SQLiteAsyncConnection connection = await _database.GetConnectionAsync().ConfigureAwait(false);

            List<Analyte> analytes = await connection.Table<Analyte>().ToListAsync().ConfigureAwait(false);

            return _cache = analytes
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
            IReadOnlyList<Analyte> all = (await GetAllAsync().ConfigureAwait(false))
                .Where(a => !a.IsHidden)
                .ToList();

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

        public async Task<IReadOnlyList<string>> GetCategoriesAsync()
        {
            IReadOnlyList<Analyte> all = await GetAllAsync().ConfigureAwait(false);

            return all
                .Select(a => a.Category)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c!.Trim())
                .Distinct(StringComparer.CurrentCultureIgnoreCase)
                .OrderBy(c => c)
                .ToList();
        }

        public async Task SetFavoriteAsync(string code, bool isFavorite)
        {
            Analyte? analyte = await FindAsync(code).ConfigureAwait(false);

            if (analyte is null || analyte.IsFavorite == isFavorite)
            {
                return;
            }

            analyte.IsFavorite = isFavorite;

            await SaveAsync(analyte).ConfigureAwait(false);
        }

        public async Task SaveAsync(Analyte analyte)
        {
            SQLiteAsyncConnection connection = await _database.GetConnectionAsync().ConfigureAwait(false);

            await connection.InsertOrReplaceAsync(analyte).ConfigureAwait(false);

            _cache = null;
        }

        public async Task DeleteAsync(string code)
        {
            SQLiteAsyncConnection connection = await _database.GetConnectionAsync().ConfigureAwait(false);

            await connection.ExecuteAsync("delete from analytes where Code = ?", code).ConfigureAwait(false);

            _cache = null;
        }
    }
}
