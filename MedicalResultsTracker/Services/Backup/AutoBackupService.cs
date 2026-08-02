using System.Globalization;
using MedicalResultsTracker.Model;
using MedicalResultsTracker.Services.Database;
using MedicalResultsTracker.Services.Export;

#if ANDROID
using Android.Content;
using Android.Provider;
#endif

namespace MedicalResultsTracker.Services.Backup
{
    /// <inheritdoc cref="IAutoBackupService"/>
    public sealed class AutoBackupService : IAutoBackupService
    {
        private const string FolderKey = "backup.auto.folder";
        private const string NameKey = "backup.auto.folder.name";
        private const string SignatureKey = "backup.auto.signature";
        private const string LastKey = "backup.auto.last.utc";

        private readonly IBloodTestRepository _tests;
        private readonly IBloodPressureRepository _pressure;
        private readonly IExportService _export;

        public AutoBackupService(
            IBloodTestRepository tests,
            IBloodPressureRepository pressure,
            IExportService export)
        {
            _tests = tests;
            _pressure = pressure;
            _export = export;
        }

        public bool IsConfigured => !string.IsNullOrEmpty(Preferences.Default.Get(FolderKey, string.Empty));

        public string? FolderName
        {
            get
            {
                string name = Preferences.Default.Get(NameKey, string.Empty);

                return string.IsNullOrEmpty(name) ? null : name;
            }
        }

        public DateTime? LastBackupUtc =>
            DateTime.TryParse(
                Preferences.Default.Get(LastKey, string.Empty),
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out DateTime last)
                ? last
                : null;

        public void Forget()
        {
#if ANDROID
            string stored = Preferences.Default.Get(FolderKey, string.Empty);

            if (!string.IsNullOrEmpty(stored))
            {
                try
                {
                    Android.App.Application.Context.ContentResolver?.ReleasePersistableUriPermission(
                        Android.Net.Uri.Parse(stored)!,
                        ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);
                }
                catch (Exception exception)
                {
                    // Разрешение могло исчезнуть само — забыть папку это не мешает.
                    Debug.WriteLine($"[MedicalResultsTracker] Не удалось снять разрешение на папку: {exception}");
                }
            }
#endif

            Preferences.Default.Remove(FolderKey);
            Preferences.Default.Remove(NameKey);
            Preferences.Default.Remove(SignatureKey);
            Preferences.Default.Remove(LastKey);
        }

        public async Task<bool> ChooseFolderAsync()
        {
#if ANDROID
            Android.Net.Uri? tree = await FolderPicker.PickAsync();

            if (tree is null)
            {
                return false;
            }

            Preferences.Default.Set(FolderKey, tree.ToString());
            Preferences.Default.Set(NameKey, FolderPicker.Describe(tree));

            // Отпечаток сбрасываем: для новой папки прошлых копий нет,
            // и первая же проверка должна записать файл.
            Preferences.Default.Remove(SignatureKey);

            return true;
#else
            await Task.CompletedTask;

            return false;
#endif
        }

        public async Task<bool> BackupIfChangedAsync()
        {
            if (!IsConfigured)
            {
                return false;
            }

            BackupSignature current = await ComputeSignatureAsync().ConfigureAwait(false);

            if (BackupSignature.Parse(Preferences.Default.Get(SignatureKey, string.Empty)) == current)
            {
                return false;
            }

            return await WriteAsync(current).ConfigureAwait(false);
        }

        public async Task<bool> BackupNowAsync()
        {
            if (!IsConfigured)
            {
                return false;
            }

            return await WriteAsync(await ComputeSignatureAsync().ConfigureAwait(false)).ConfigureAwait(false);
        }

        /// <summary>
        /// Состояние данных одним значением. Считается по тем же двум наборам, что попадают
        /// в копию: анализы и давление.
        /// </summary>
        private async Task<BackupSignature> ComputeSignatureAsync()
        {
            IReadOnlyList<BloodTest> tests = await _tests.GetAllAsync().ConfigureAwait(false);
            IReadOnlyList<BloodPressureReading> pressure = await _pressure.GetAllAsync().ConfigureAwait(false);

            long ticks = 0;

            foreach (BloodTest test in tests)
            {
                ticks = Math.Max(ticks, test.ModifiedUtc.Ticks);
            }

            foreach (BloodPressureReading reading in pressure)
            {
                ticks = Math.Max(ticks, reading.ModifiedUtc.Ticks);
            }

            return new BackupSignature(tests.Count, pressure.Count, ticks);
        }

        private async Task<bool> WriteAsync(BackupSignature signature)
        {
            // Файл собирается тем же кодом, что и ручная выгрузка: формат один,
            // и копии взаимозаменяемы с теми, что человек делает сам.
            string source = await _export.ExportBackupAsync().ConfigureAwait(false);

            string name = $"medical-results-backup-{DateTime.Now:yyyy-MM-dd-HHmm}.json";

#if ANDROID
            string folder = Preferences.Default.Get(FolderKey, string.Empty);

            if (!FolderPicker.TryWrite(Android.Net.Uri.Parse(folder)!, name, source))
            {
                return false;
            }
#else
            await Task.CompletedTask;

            return false;
#endif

            Preferences.Default.Set(SignatureKey, signature.ToString());
            Preferences.Default.Set(LastKey, DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));

            return true;
        }
    }
}
