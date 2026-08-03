using MedicalResultsTracker.Model;
using SQLite;

namespace MedicalResultsTracker.Services.Database
{
    /// <inheritdoc cref="IBloodTestRepository"/>
    public sealed class BloodTestRepository : IBloodTestRepository
    {
        private readonly IMedicalDatabase _database;

        public BloodTestRepository(IMedicalDatabase database)
        {
            _database = database;
        }

        public async Task<IReadOnlyList<BloodTest>> GetAllAsync()
        {
            SQLiteAsyncConnection connection = await _database.GetConnectionAsync().ConfigureAwait(false);

            List<BloodTest> tests = await connection.Table<BloodTest>()
                .OrderByDescending(t => t.Date)
                .ToListAsync()
                .ConfigureAwait(false);

            if (tests.Count == 0)
            {
                return tests;
            }

            List<BloodParameter> parameters = await connection.Table<BloodParameter>()
                .ToListAsync()
                .ConfigureAwait(false);

            ILookup<Guid, BloodParameter> byTest = parameters.ToLookup(p => p.TestId);

            foreach (BloodTest test in tests)
            {
                test.Parameters = byTest[test.Id].OrderBy(p => p.SortOrder).ThenBy(p => p.Name).ToList();
            }

            return tests;
        }

        public async Task<BloodTest?> GetAsync(Guid id)
        {
            SQLiteAsyncConnection connection = await _database.GetConnectionAsync().ConfigureAwait(false);

            BloodTest? test = await connection.Table<BloodTest>()
                .Where(t => t.Id == id)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            if (test is not null)
            {
                await LoadParametersAsync(connection, test).ConfigureAwait(false);
            }

            return test;
        }

        public async Task<BloodTest?> GetLatestAsync()
        {
            SQLiteAsyncConnection connection = await _database.GetConnectionAsync().ConfigureAwait(false);

            BloodTest? test = await connection.Table<BloodTest>()
                .OrderByDescending(t => t.Date)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            if (test is not null)
            {
                await LoadParametersAsync(connection, test).ConfigureAwait(false);
            }

            return test;
        }

        public async Task<BloodTest?> GetPreviousAsync(DateTime beforeDate)
        {
            SQLiteAsyncConnection connection = await _database.GetConnectionAsync().ConfigureAwait(false);

            BloodTest? test = await connection.Table<BloodTest>()
                .Where(t => t.Date < beforeDate)
                .OrderByDescending(t => t.Date)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            if (test is not null)
            {
                await LoadParametersAsync(connection, test).ConfigureAwait(false);
            }

            return test;
        }

        public async Task<BloodTest?> GetByDateAsync(DateTime date, Guid exceptId)
        {
            SQLiteAsyncConnection connection = await _database.GetConnectionAsync().ConfigureAwait(false);

            // Сравниваем сутками, а не значением: время в дате может быть каким угодно.
            DateTime from = date.Date;
            DateTime to = from.AddDays(1);

            List<BloodTest> sameDay = await connection.Table<BloodTest>()
                .Where(t => t.Date >= from && t.Date < to)
                .ToListAsync()
                .ConfigureAwait(false);

            BloodTest? test = sameDay
                .Where(t => t.Id != exceptId)
                .OrderBy(t => t.ModifiedUtc)
                .FirstOrDefault();

            if (test is not null)
            {
                await LoadParametersAsync(connection, test).ConfigureAwait(false);
            }

            return test;
        }

        public async Task SaveAsync(BloodTest test)
        {
            SQLiteAsyncConnection connection = await _database.GetConnectionAsync().ConfigureAwait(false);

            test.ModifiedUtc = DateTime.UtcNow;

            for (int i = 0; i < test.Parameters.Count; i++)
            {
                BloodParameter parameter = test.Parameters[i];
                parameter.TestId = test.Id;
                parameter.SortOrder = i;
            }

            // Строки показателей переписываем целиком: пользователь мог удалить строку в редакторе.
            await connection.RunInTransactionAsync(db =>
            {
                db.InsertOrReplace(test);
                db.Execute("delete from blood_parameters where TestId = ?", test.Id);

                foreach (BloodParameter parameter in test.Parameters)
                {
                    db.Insert(parameter);
                }
            }).ConfigureAwait(false);
        }

        public async Task DeleteAsync(Guid id)
        {
            SQLiteAsyncConnection connection = await _database.GetConnectionAsync().ConfigureAwait(false);

            await connection.RunInTransactionAsync(db =>
            {
                db.Execute("delete from blood_parameters where TestId = ?", id);
                db.Execute("delete from blood_tests where Id = ?", id);
            }).ConfigureAwait(false);
        }

        public async Task DeleteAllAsync()
        {
            SQLiteAsyncConnection connection = await _database.GetConnectionAsync().ConfigureAwait(false);

            await connection.RunInTransactionAsync(db =>
            {
                db.Execute("delete from blood_parameters");
                db.Execute("delete from blood_tests");
            }).ConfigureAwait(false);
        }

        public async Task<int> CountAsync()
        {
            SQLiteAsyncConnection connection = await _database.GetConnectionAsync().ConfigureAwait(false);

            return await connection.Table<BloodTest>().CountAsync().ConfigureAwait(false);
        }

        public async Task<IReadOnlyDictionary<string, int>> GetUsageByCodeAsync()
        {
            SQLiteAsyncConnection connection = await _database.GetConnectionAsync().ConfigureAwait(false);

            List<CodeUsage> usage = await connection.QueryAsync<CodeUsage>(
                "select Code, count(*) as Total from blood_parameters where Code is not null and Code <> '' group by Code")
                .ConfigureAwait(false);

            return usage.ToDictionary(u => u.Code!, u => u.Total, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<int> ReassignCodeAsync(string fromCode, string toCode)
        {
            SQLiteAsyncConnection connection = await _database.GetConnectionAsync().ConfigureAwait(false);

            return await connection
                .ExecuteAsync("update blood_parameters set Code = ? where Code = ?", toCode, fromCode)
                .ConfigureAwait(false);
        }

        public async Task<int> SetRangeAsync(string code, double? min, double? max)
        {
            SQLiteAsyncConnection connection = await _database.GetConnectionAsync().ConfigureAwait(false);

            return await connection
                .ExecuteAsync("update blood_parameters set RefMin = ?, RefMax = ? where Code = ?", min, max, code)
                .ConfigureAwait(false);
        }

        private static async Task LoadParametersAsync(SQLiteAsyncConnection connection, BloodTest test)
        {
            Guid testId = test.Id;

            List<BloodParameter> parameters = await connection.Table<BloodParameter>()
                .Where(p => p.TestId == testId)
                .ToListAsync()
                .ConfigureAwait(false);

            test.Parameters = parameters.OrderBy(p => p.SortOrder).ThenBy(p => p.Name).ToList();
        }

        /// <summary>Строка результата агрегирующего запроса — sqlite-net раскладывает его по свойствам.</summary>
        private sealed class CodeUsage
        {
            public string? Code { get; set; }

            public int Total { get; set; }
        }
    }
}
