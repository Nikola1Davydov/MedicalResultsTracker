using Android.Content;
using Android.Provider;

namespace MedicalResultsTracker.Services.Backup
{
    /// <summary>
    /// Выбор папки и запись в неё через Storage Access Framework.
    ///
    /// Разрешение выдаётся на одну конкретную папку и один раз — в манифесте никаких
    /// разрешений на файлы не появляется. Приложение не может ни прочитать, ни записать
    /// ничего за её пределами.
    /// </summary>
    internal static class FolderPicker
    {
        /// <summary>Произвольный код запроса: важно лишь, чтобы он не совпал с чужими.</summary>
        private const int RequestCode = 4711;

        private static TaskCompletionSource<Android.Net.Uri?>? _pending;

        public static Task<Android.Net.Uri?> PickAsync()
        {
            if (Platform.CurrentActivity is not Android.App.Activity activity)
            {
                return Task.FromResult<Android.Net.Uri?>(null);
            }

            // Второй одновременный выбор невозможен, но если он как-то случится —
            // прежнее ожидание нужно закрыть, иначе оно повиснет навсегда.
            _pending?.TrySetResult(null);
            _pending = new TaskCompletionSource<Android.Net.Uri?>();

            Intent intent = new(Intent.ActionOpenDocumentTree);
            intent.AddFlags(
                ActivityFlags.GrantReadUriPermission |
                ActivityFlags.GrantWriteUriPermission |
                ActivityFlags.GrantPersistableUriPermission);

            activity.StartActivityForResult(intent, RequestCode);

            return _pending.Task;
        }

        /// <summary>Вызывается из MainActivity: системный диалог возвращает результат туда.</summary>
        public static bool HandleResult(int requestCode, Result resultCode, Intent? data)
        {
            if (requestCode != RequestCode)
            {
                return false;
            }

            TaskCompletionSource<Android.Net.Uri?>? pending = _pending;
            _pending = null;

            if (pending is null)
            {
                return true;
            }

            Android.Net.Uri? tree = resultCode == Result.Ok ? data?.Data : null;

            if (tree is not null)
            {
                // Без этого разрешение пропадёт при перезапуске приложения.
                Android.App.Application.Context.ContentResolver?.TakePersistableUriPermission(
                    tree,
                    ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);
            }

            pending.TrySetResult(tree);

            return true;
        }

        /// <summary>Имя папки для показа в настройках. Полный URI человеку ничего не говорит.</summary>
        public static string Describe(Android.Net.Uri tree)
        {
            string? id = DocumentsContract.GetTreeDocumentId(tree);

            if (string.IsNullOrEmpty(id))
            {
                return tree.LastPathSegment ?? tree.ToString()!;
            }

            // Идентификатор обычно выглядит как «primary:Documents/Backups».
            int colon = id.IndexOf(':');

            return colon >= 0 && colon + 1 < id.Length ? id[(colon + 1)..] : id;
        }

        /// <summary>
        /// Кладёт файл в выбранную папку. false — если разрешение отозвано или папки больше нет:
        /// это не повод падать, автосохранение просто не состоится и будет видно в настройках.
        /// </summary>
        public static bool TryWrite(Android.Net.Uri tree, string fileName, string sourcePath)
        {
            try
            {
                ContentResolver? resolver = Android.App.Application.Context.ContentResolver;

                if (resolver is null)
                {
                    return false;
                }

                string? treeId = DocumentsContract.GetTreeDocumentId(tree);

                if (string.IsNullOrEmpty(treeId))
                {
                    return false;
                }

                Android.Net.Uri? folder = DocumentsContract.BuildDocumentUriUsingTree(tree, treeId);

                if (folder is null)
                {
                    return false;
                }

                Android.Net.Uri? file = DocumentsContract.CreateDocument(
                    resolver, folder, "application/json", fileName);

                if (file is null)
                {
                    return false;
                }

                using Stream? target = resolver.OpenOutputStream(file);

                if (target is null)
                {
                    return false;
                }

                using FileStream source = File.OpenRead(sourcePath);
                source.CopyTo(target);

                return true;
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"[MedicalResultsTracker] Не удалось записать копию в папку: {exception}");

                return false;
            }
        }
    }
}
