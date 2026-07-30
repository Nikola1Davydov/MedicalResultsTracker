using MedicalResultsTracker.Services.Ai;
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
        private bool _allowDocumentRecognition;

        [ObservableProperty]
        private bool _allowResultCommentary;

        /// <summary>Защита от рекурсии: при загрузке переключатели ставятся программно.</summary>
        private bool _suppressConsentUpdates;

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

            Title = "Настройки";
        }

        public override Task InitializeAsync() => RunAsync(LoadAsync, "Не удалось открыть настройки");

        [RelayCommand]
        private Task ExportMatrix() => RunAsync(async () =>
        {
            string path = await _export.ExportMatrixCsvAsync();
            await _export.ShareAsync(path, "Результаты анализов (таблица)");
        }, "Не удалось выгрузить таблицу");

        [RelayCommand]
        private Task ExportFlat() => RunAsync(async () =>
        {
            string path = await _export.ExportFlatCsvAsync();
            await _export.ShareAsync(path, "Результаты анализов (список)");
        }, "Не удалось выгрузить список");

        [RelayCommand]
        private Task ExportBackup() => RunAsync(async () =>
        {
            string path = await _export.ExportBackupAsync();
            await _export.ShareAsync(path, "Резервная копия истории");
        }, "Не удалось создать резервную копию");

        [RelayCommand]
        private Task ImportBackup() => RunAsync(async () =>
        {
            FileResult? file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Выберите файл резервной копии",
            });

            if (file is null)
            {
                return;
            }

            int imported = await _export.ImportBackupAsync(file.FullPath);

            await Dialog.AlertAsync("Импорт завершён", imported == 0
                ? "Новых анализов в файле не найдено."
                : $"Добавлено анализов: {imported}.");

            await LoadAsync();
        }, "Не удалось импортировать резервную копию");

        [RelayCommand]
        private Task DeleteAllData() => RunAsync(async () =>
        {
            bool confirmed = await Dialog.ConfirmAsync(
                "Удалить всю историю?",
                "Все анализы будут удалены с устройства. Действие необратимо — сначала сделайте резервную копию.",
                "Удалить");

            if (!confirmed)
            {
                return;
            }

            await _repository.DeleteAllAsync();
            await LoadAsync();
        }, "Не удалось очистить историю");

        [RelayCommand]
        private Task ShareForAi() => RunAsync(async () =>
        {
            string text = await _export.BuildTextSummaryAsync();
            await _export.ShareTextAsync(text, "Мои анализы");
        }, "Не удалось подготовить текст");

        [RelayCommand]
        private Task CopyForAi() => RunAsync(async () =>
        {
            string text = await _export.BuildTextSummaryAsync();

            await _export.CopyToClipboardAsync(text);
            await Dialog.AlertAsync("Скопировано", "Таблица в буфере обмена — вставьте её в любой чат.");
        }, "Не удалось скопировать текст");

        partial void OnAllowDocumentRecognitionChanged(bool value) =>
            UpdateConsent(AiConsentScope.DocumentRecognition, value);

        partial void OnAllowResultCommentaryChanged(bool value) =>
            UpdateConsent(AiConsentScope.ResultCommentary, value);

        private void UpdateConsent(AiConsentScope scope, bool granted)
        {
            if (_suppressConsentUpdates)
            {
                return;
            }

            if (granted)
            {
                _consent.Grant(scope, _assistant.ProviderName);
            }
            else
            {
                _consent.Revoke(scope);
            }

            UpdateAssistantSummary();
        }

        private async Task LoadAsync()
        {
            DatabasePath = _database.DatabasePath;

            int count = await _repository.CountAsync();

            StorageSummary = count switch
            {
                0 => "История пуста. Данные хранятся только на этом устройстве.",
                1 => "1 анализ. Данные хранятся только на этом устройстве.",
                _ => $"{count} анализов. Данные хранятся только на этом устройстве."
            };

            _suppressConsentUpdates = true;
            AllowDocumentRecognition = _consent.IsAllowed(AiConsentScope.DocumentRecognition);
            AllowResultCommentary = _consent.IsAllowed(AiConsentScope.ResultCommentary);
            _suppressConsentUpdates = false;

            UpdateAssistantSummary();
        }

        private void UpdateAssistantSummary()
        {
            if (_consent.Current.Scope == AiConsentScope.None)
            {
                AssistantSummary = "Всё выключено: приложение не отправляет наружу ни одного байта.";
                return;
            }

            AssistantSummary = $"Разрешения выданы для «{_assistant.ProviderName}»" +
                (_consent.Current.GrantedUtc is DateTime granted
                    ? $" {granted.ToLocalTime():dd.MM.yyyy}."
                    : ".") +
                " Провайдер в этой сборке ещё не подключён, поэтому фактической отправки не происходит.";
        }


    }
}
