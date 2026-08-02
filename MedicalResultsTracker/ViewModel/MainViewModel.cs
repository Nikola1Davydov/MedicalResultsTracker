using System.Globalization;
using MedicalResultsTracker.Controls;
using MedicalResultsTracker.Model;
using MedicalResultsTracker.Resources.Strings;
using MedicalResultsTracker.Services.Ai;
using MedicalResultsTracker.Services.Analysis;
using MedicalResultsTracker.Services.Backup;
using MedicalResultsTracker.Services.Database;
using MedicalResultsTracker.Services.Export;
using MedicalResultsTracker.Services.UI;

namespace MedicalResultsTracker.ViewModel
{
    /// <summary>Главный экран: что было в последний раз и что изменилось.</summary>
    public partial class MainViewModel : BaseViewModel
    {
        private readonly IBloodTestRepository _repository;
        private readonly IBloodPressureRepository _pressure;
        private readonly IAutoBackupService _backup;
        private readonly IAnalysisService _analysis;
        private readonly IExportService _export;
        private readonly IAiConsentService _consent;
        private readonly IAiAssistant _assistant;

        [ObservableProperty]
        private string _lastTestTitle = S.Dash_NoTests;

        [ObservableProperty]
        private string _summary = S.Dash_EmptyHint;

        [ObservableProperty]
        private bool _hasData;

        [ObservableProperty]
        private bool _isEmpty = true;

        [ObservableProperty]
        private bool _hasAttention;

        [ObservableProperty]
        private bool _hasChanges;

        [ObservableProperty]
        private string _assistantStatus = string.Empty;

        /// <summary>Последнее измерение давления — «130/85 · сегодня, 08:20» либо приглашение начать.</summary>
        [ObservableProperty]
        private string _pressureSummary = string.Empty;

        [ObservableProperty]
        private Color _pressureColor = StatusPalette.Unknown;

        private Guid? _latestTestId;

        /// <summary>За один запуск копия делается один раз, а не на каждое открытие вкладки.</summary>
        private bool _backupChecked;

        public MainViewModel(
            IBloodTestRepository repository,
            IBloodPressureRepository pressure,
            IAutoBackupService backup,
            IAnalysisService analysis,
            IExportService export,
            IAiConsentService consent,
            IAiAssistant assistant)
        {
            _repository = repository;
            _pressure = pressure;
            _backup = backup;
            _analysis = analysis;
            _export = export;
            _consent = consent;
            _assistant = assistant;

            Title = S.Dash_Title;
        }

        /// <summary>Показатели, вышедшие за норму, — их хочется видеть первыми.</summary>
        public ObservableCollection<TrendItemViewModel> Attention { get; } = new();

        /// <summary>Заметно изменившиеся показатели, даже если они в норме.</summary>
        public ObservableCollection<TrendItemViewModel> Changes { get; } = new();

        public override Task InitializeAsync() => RunAsync(LoadAsync, S.Err_Load);

        [RelayCommand]
        private Task Refresh() => RunAsync(LoadAsync, S.Err_Refresh);

        [RelayCommand]
        private Task AddTest() => Shell.Current.GoToAsync(AppRoutes.TestEdit);

        /// <summary>Давление записывают ежедневно, поэтому кнопка ведёт сразу в форму, а не в список.</summary>
        [RelayCommand]
        private Task AddPressure() => Shell.Current.GoToAsync(AppRoutes.PressureEdit);

        [RelayCommand]
        private Task OpenPressure() => Shell.Current.GoToAsync(AppRoutes.Pressure);

        [RelayCommand]
        private Task OpenLastTest() => _latestTestId is Guid id
            ? Shell.Current.GoToAsync($"{AppRoutes.TestEdit}?{AppRoutes.TestIdParameter}={id}")
            : Shell.Current.GoToAsync(AppRoutes.TestEdit);

        [RelayCommand]
        private Task OpenTrend(TrendItemViewModel? item) => item is null
            ? Task.CompletedTask
            : OpenSeriesAsync(item.Key);

        private static Task OpenSeriesAsync(string key) => Shell.Current.GoToAsync(
            $"{AppRoutes.TrendDetail}?{AppRoutes.SeriesKeyParameter}={Uri.EscapeDataString(key)}");

        /// <summary>
        /// Готовит таблицу текстом и открывает системный диалог «Поделиться».
        /// Приложение не выбирает получателя и никуда само не отправляет: в какое приложение
        /// уйдёт текст, решает пользователь в системном списке.
        /// </summary>
        [RelayCommand]
        private Task AskAi() => RunAsync(async () =>
        {
            string text = await _export.BuildTextSummaryAsync();
            await _export.ShareTextAsync(text, S.Dash_Title);
        }, S.Err_Text);

        private async Task LoadAsync()
        {
            Title = S.Dash_Title;

            IReadOnlyList<ParameterTrend> trends = await _analysis.GetLatestTrendsAsync();
            BloodTest? latest = await _repository.GetLatestAsync();
            int count = await _repository.CountAsync();

            _latestTestId = latest?.Id;
            HasData = latest is not null;
            IsEmpty = latest is null;

            LastTestTitle = latest is null
                ? S.Dash_NoTests
                : string.Format(S.Dash_LastTest, latest.Title);

            Attention.Clear();
            Changes.Clear();

            foreach (ParameterTrend trend in trends
                .Where(t => t.Status is ParameterStatus.Low or ParameterStatus.High)
                .OrderByDescending(t => t.Assessment == TrendAssessment.Worsened)
                .ThenBy(t => t.Name))
            {
                Attention.Add(new TrendItemViewModel(trend, _analysis.GetKey(trend.Current)));
            }

            foreach (ParameterTrend trend in trends
                .Where(t => t.Assessment is TrendAssessment.Improved or TrendAssessment.Worsened)
                .OrderByDescending(t => Math.Abs(t.DeltaPercent ?? 0))
                .Take(5))
            {
                Changes.Add(new TrendItemViewModel(trend, _analysis.GetKey(trend.Current)));
            }

            HasAttention = Attention.Count > 0;
            HasChanges = Changes.Count > 0;

            Summary = latest is null
                ? S.Dash_EmptyHint
                : BuildSummary(latest, count, Attention.Count);

            await LoadPressureAsync();

            // Копия делается один раз за запуск и только если данные изменились.
            // Молча и в фоне: это обслуживание, а не действие пользователя, и мешать ему нечем.
            _ = BackupQuietlyAsync();

            AssistantStatus = _consent.Current.Scope == AiConsentScope.None
                ? S.Dash_AiOff
                : string.Format(S.Dash_AiOn, _assistant.ProviderName);
        }

        /// <summary>
        /// Автосохранение не должно ни задерживать экран, ни показывать ошибок: папка могла
        /// исчезнуть, разрешение — быть отозвано, и человек узнает об этом в настройках,
        /// где видно дату последней копии, а не всплывающим окном на главной.
        /// </summary>
        private async Task BackupQuietlyAsync()
        {
            if (_backupChecked)
            {
                return;
            }

            _backupChecked = true;

            try
            {
                await _backup.BackupIfChangedAsync();
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"[MedicalResultsTracker] Автосохранение не удалось: {exception}");
            }
        }

        private async Task LoadPressureAsync()
        {
            if (await _pressure.GetLatestAsync() is not BloodPressureReading latest)
            {
                PressureSummary = S.Bp_NoneYet;
                PressureColor = StatusPalette.Unknown;
                return;
            }

            PressureSummary = string.Format(
                S.Bp_LastReading,
                latest.Display,
                latest.MeasuredAt.ToString("g", CultureInfo.CurrentCulture));

            PressureColor = latest.IsAbove(BloodPressureTarget.Systolic, BloodPressureTarget.Diastolic)
                ? StatusPalette.High
                : StatusPalette.Normal;
        }

        private static string BuildSummary(BloodTest latest, int totalTests, int attentionCount)
        {
            string tests = totalTests == 1 ? S.Dash_OneTest : string.Format(S.Dash_ManyTests, totalTests);

            string attention = attentionCount switch
            {
                0 => S.Dash_AllInRange,
                1 => S.Dash_OneOut,
                _ => string.Format(S.Dash_ManyOut, attentionCount)
            };

            return string.Format(S.Dash_Summary, latest.Parameters.Count, attention, tests);
        }
    }
}
