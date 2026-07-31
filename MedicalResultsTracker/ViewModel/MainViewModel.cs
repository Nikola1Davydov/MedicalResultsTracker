using MedicalResultsTracker.Model;
using MedicalResultsTracker.Resources.Strings;
using MedicalResultsTracker.Services.Ai;
using MedicalResultsTracker.Services.Analysis;
using MedicalResultsTracker.Services.Database;
using MedicalResultsTracker.Services.Export;

namespace MedicalResultsTracker.ViewModel
{
    /// <summary>Главный экран: что было в последний раз и что изменилось.</summary>
    public partial class MainViewModel : BaseViewModel
    {
        private readonly IBloodTestRepository _repository;
        private readonly IAnalysisService _analysis;
        private readonly IAnalyteCatalog _catalog;
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
        private bool _hasFavorites;

        /// <summary>Подсказка про избранное показывается, только когда данные есть, а отмеченного ничего нет.</summary>
        [ObservableProperty]
        private bool _showFavoritesHint;

        [ObservableProperty]
        private string _assistantStatus = string.Empty;

        private Guid? _latestTestId;

        public MainViewModel(
            IBloodTestRepository repository,
            IAnalysisService analysis,
            IAnalyteCatalog catalog,
            IExportService export,
            IAiConsentService consent,
            IAiAssistant assistant)
        {
            _repository = repository;
            _analysis = analysis;
            _catalog = catalog;
            _export = export;
            _consent = consent;
            _assistant = assistant;

            Title = S.Dash_Title;
        }

        /// <summary>
        /// Показатели, отмеченные звёздочкой. Берутся из всей истории, а не из последнего анализа:
        /// следят обычно за тем, что сдают не каждый раз.
        /// </summary>
        public ObservableCollection<SeriesItemViewModel> Favorites { get; } = new();

        /// <summary>Показатели, вышедшие за норму, — их хочется видеть первыми.</summary>
        public ObservableCollection<TrendItemViewModel> Attention { get; } = new();

        /// <summary>Заметно изменившиеся показатели, даже если они в норме.</summary>
        public ObservableCollection<TrendItemViewModel> Changes { get; } = new();

        public override Task InitializeAsync() => RunAsync(LoadAsync, S.Err_Load);

        [RelayCommand]
        private Task Refresh() => RunAsync(LoadAsync, S.Err_Refresh);

        [RelayCommand]
        private Task AddTest() => Shell.Current.GoToAsync(AppRoutes.TestEdit);

        [RelayCommand]
        private Task OpenLastTest() => _latestTestId is Guid id
            ? Shell.Current.GoToAsync($"{AppRoutes.TestEdit}?{AppRoutes.TestIdParameter}={id}")
            : Shell.Current.GoToAsync(AppRoutes.TestEdit);

        [RelayCommand]
        private Task OpenHistory() => Shell.Current.GoToAsync(AppRoutes.History);

        [RelayCommand]
        private Task OpenTrends() => Shell.Current.GoToAsync(AppRoutes.Trends);

        [RelayCommand]
        private Task OpenTrend(TrendItemViewModel? item) => item is null
            ? Task.CompletedTask
            : OpenSeriesAsync(item.Key);

        [RelayCommand]
        private Task OpenFavorite(SeriesItemViewModel? item) => item is null
            ? Task.CompletedTask
            : OpenSeriesAsync(item.Key);

        [RelayCommand]
        private Task OpenCatalog() => Shell.Current.GoToAsync(AppRoutes.Catalog);

        private static Task OpenSeriesAsync(string key) => Shell.Current.GoToAsync(
            $"{AppRoutes.TrendDetail}?{AppRoutes.SeriesKeyParameter}={Uri.EscapeDataString(key)}");

        [RelayCommand]
        private Task Export() => RunAsync(async () =>
        {
            string path = await _export.ExportMatrixCsvAsync();
            await _export.ShareAsync(path, S.Share_Results);
        }, S.Err_Export);

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
            Favorites.Clear();

            IReadOnlyList<Analyte> catalog = await _catalog.GetAllAsync();
            HashSet<string> favoriteCodes = catalog
                .Where(a => a.IsFavorite)
                .Select(a => a.Code)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (favoriteCodes.Count > 0)
            {
                Dictionary<string, Analyte> byCode = catalog
                    .GroupBy(a => a.Code, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                foreach (ParameterSeries series in (await _analysis.GetSeriesAsync())
                    .Where(s => favoriteCodes.Contains(s.Key))
                    .OrderBy(s => s.Name))
                {
                    Favorites.Add(new SeriesItemViewModel(series, byCode.GetValueOrDefault(series.Key)));
                }
            }

            HasFavorites = Favorites.Count > 0;
            ShowFavoritesHint = HasData && !HasFavorites;

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

            AssistantStatus = _consent.Current.Scope == AiConsentScope.None
                ? S.Dash_AiOff
                : string.Format(S.Dash_AiOn, _assistant.ProviderName);
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
