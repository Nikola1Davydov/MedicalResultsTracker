using System.Globalization;
using MedicalResultsTracker.Services.Ai;
using MedicalResultsTracker.Resources.Strings;
using MedicalResultsTracker.Services.UI;
using MedicalResultsTracker.Services.Database;
using MedicalResultsTracker.Services.Export;

namespace MedicalResultsTracker.ViewModel
{
    /// <summary>Хранение данных, выгрузка и разрешения для ИИ-помощника.</summary>
    public partial class SettingsViewModel : BaseViewModel
    {
        private readonly IMedicalDatabase _database;
        private readonly IBloodTestRepository _repository;
        private readonly IExportService _export;
        private readonly IAiConsentService _consent;
        private readonly IAiAssistant _assistant;

        [ObservableProperty]
        private string _databasePath = string.Empty;

        [ObservableProperty]
        private string _storageSummary = string.Empty;

        [ObservableProperty]
        private string _assistantSummary = string.Empty;

        [ObservableProperty]
        private string _versionSummary = string.Empty;

        [ObservableProperty]
        private LanguageOption? _selectedLanguage;

        /// <summary>Защита от рекурсии: при загрузке язык проставляется программно.</summary>
        private bool _suppressLanguageChange;

        public SettingsViewModel(
            IMedicalDatabase database,
            IBloodTestRepository repository,
            IExportService export,
            IAiConsentService consent,
            IAiAssistant assistant)
        {
            _database = database;
            _repository = repository;
            _export = export;
            _consent = consent;
            _assistant = assistant;

            Title = S.Tab_Settings;
        }

        public IReadOnlyList<LanguageOption> Languages => Localization.Available;

        public override Task InitializeAsync() => RunAsync(LoadAsync, S.Err_Settings);

        partial void OnSelectedLanguageChanged(LanguageOption? value)
        {
            if (_suppressLanguageChange || value is null || value.Code == Localization.Current.SelectedCode)
            {
                return;
            }

            Localization.Current.SetLanguage(value.Code);

            // Разметка читает строки в момент построения экрана, и уведомления об изменении
            // недостаточно: часть текстов уже «застыла» в свойствах. Поэтому оболочка
            // пересобирается целиком — так язык меняется гарантированно и весь сразу.
            if (Application.Current?.Windows.FirstOrDefault() is Window window)
            {
                window.Page = new AppShell();

                // Пересборка возвращает на первую вкладку, а пользователь стоял в настройках —
                // возвращаем его туда. Переход ставится в очередь: оболочка к этому моменту
                // ещё не подключена, и Shell.Current указывал бы на старую.
                Application.Current.Dispatcher.Dispatch(
                    () => _ = Shell.Current.GoToAsync(AppRoutes.Settings));
            }
        }

        [RelayCommand]
        private Task ExportMatrix() => RunAsync(async () =>
        {
            string path = await _export.ExportMatrixCsvAsync();
            await _export.ShareAsync(path, S.Share_ResultsTable);
        }, S.Err_Export);

        [RelayCommand]
        private Task ExportFlat() => RunAsync(async () =>
        {
            string path = await _export.ExportFlatCsvAsync();
            await _export.ShareAsync(path, S.Share_ResultsList);
        }, S.Err_ExportList);

        [RelayCommand]
        private Task ExportBackup() => RunAsync(async () =>
        {
            string path = await _export.ExportBackupAsync();
            await _export.ShareAsync(path, S.Share_Backup);
        }, S.Err_Backup);

        [RelayCommand]
        private Task ImportBackup() => RunAsync(async () =>
        {
            FileResult? file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = S.Set_ImportPick,
            });

            if (file is null)
            {
                return;
            }

            int imported = await _export.ImportBackupAsync(file.FullPath);

            await Dialog.AlertAsync(S.Set_ImportDoneTitle, imported == 0
                ? S.Set_ImportNothing
                : string.Format(S.Set_ImportCount, imported));

            await LoadAsync();
        }, S.Err_Import);

        [RelayCommand]
        private Task DeleteAllData() => RunAsync(async () =>
        {
            bool confirmed = await Dialog.ConfirmAsync(
                S.Set_DeleteAllTitle,
                S.Set_DeleteAllBody,
                S.Common_Delete);

            if (!confirmed)
            {
                return;
            }

            await _repository.DeleteAllAsync();
            await LoadAsync();
        }, S.Err_DeleteAll);

        [RelayCommand]
        private Task OpenCatalog() => Shell.Current.GoToAsync(AppRoutes.Catalog);

        [RelayCommand]
        private Task OpenHistory() => Shell.Current.GoToAsync(AppRoutes.History);

        [RelayCommand]
        private Task ShareForAi() => RunAsync(async () =>
        {
            string text = await _export.BuildTextSummaryAsync();
            await _export.ShareTextAsync(text, S.Dash_Title);
        }, S.Err_Text);

        [RelayCommand]
        private Task CopyForAi() => RunAsync(async () =>
        {
            string text = await _export.BuildTextSummaryAsync();

            await _export.CopyToClipboardAsync(text);
            await Dialog.AlertAsync(S.Set_CopiedTitle, S.Set_CopiedBody);
        }, S.Err_Copy);

        private async Task LoadAsync()
        {
            Title = S.Tab_Settings;

            _suppressLanguageChange = true;
            SelectedLanguage = Localization.Available.FirstOrDefault(l => l.Code == Localization.Current.SelectedCode);
            _suppressLanguageChange = false;

            DatabasePath = _database.DatabasePath;

            int count = await _repository.CountAsync();

            StorageSummary = count switch
            {
                0 => S.Set_StorageEmpty,
                1 => S.Set_StorageOne,
                _ => string.Format(S.Set_StorageMany, count)
            };

            UpdateAssistantSummary();

            // Версия ставится тегом релиза и больше нигде: увидев её здесь, можно сверить,
            // что на телефоне стоит именно тот выпуск, который лежит в Releases.
            VersionSummary = string.Format(
                S.Set_Version,
                AppInfo.Current.VersionString,
                AppInfo.Current.BuildString);
        }

        private void UpdateAssistantSummary()
        {
            if (_consent.Current.Scope == AiConsentScope.None)
            {
                AssistantSummary = S.Set_AiOff;
                return;
            }

            string when = _consent.Current.GrantedUtc is DateTime granted
                ? $" {granted.ToLocalTime().ToString("d", CultureInfo.CurrentCulture)}."
                : ".";

            AssistantSummary = string.Format(S.Set_AiGranted, _assistant.ProviderName, when);
        }


    }
}
