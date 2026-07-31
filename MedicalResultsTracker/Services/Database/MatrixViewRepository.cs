using MedicalResultsTracker.Model;
using SQLite;

namespace MedicalResultsTracker.Services.Database
{
    /// <inheritdoc cref="IMatrixViewRepository"/>
    public sealed class MatrixViewRepository : IMatrixViewRepository
    {
        private readonly IMedicalDatabase _database;

        public MatrixViewRepository(IMedicalDatabase database)
        {
            _database = database;
        }

        public async Task<IReadOnlyList<MatrixView>> GetAllAsync()
        {
            SQLiteAsyncConnection connection = await _database.GetConnectionAsync().ConfigureAwait(false);

            List<MatrixView> views = await connection.Table<MatrixView>().ToListAsync().ConfigureAwait(false);

            if (views.Count == 0)
            {
                return views;
            }

            List<MatrixViewItem> items = await connection.Table<MatrixViewItem>().ToListAsync().ConfigureAwait(false);
            ILookup<Guid, MatrixViewItem> byView = items.ToLookup(i => i.ViewId);

            foreach (MatrixView view in views)
            {
                view.Codes = byView[view.Id].OrderBy(i => i.SortOrder).Select(i => i.Code).ToList();
            }

            return views.OrderBy(v => v.SortOrder).ThenBy(v => v.Name).ToList();
        }

        public async Task<MatrixView?> GetAsync(Guid id)
        {
            IReadOnlyList<MatrixView> all = await GetAllAsync().ConfigureAwait(false);

            return all.FirstOrDefault(v => v.Id == id);
        }

        public async Task SaveAsync(MatrixView view)
        {
            SQLiteAsyncConnection connection = await _database.GetConnectionAsync().ConfigureAwait(false);

            await connection.RunInTransactionAsync(db =>
            {
                db.InsertOrReplace(view);
                db.Execute("delete from matrix_view_items where ViewId = ?", view.Id);

                for (int i = 0; i < view.Codes.Count; i++)
                {
                    db.Insert(new MatrixViewItem
                    {
                        ViewId = view.Id,
                        Code = view.Codes[i],
                        SortOrder = i,
                    });
                }
            }).ConfigureAwait(false);
        }

        public async Task DeleteAsync(Guid id)
        {
            SQLiteAsyncConnection connection = await _database.GetConnectionAsync().ConfigureAwait(false);

            await connection.RunInTransactionAsync(db =>
            {
                db.Execute("delete from matrix_view_items where ViewId = ?", id);
                db.Execute("delete from matrix_views where Id = ?", id);
            }).ConfigureAwait(false);
        }
    }
}
