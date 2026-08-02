using MedicalResultsTracker.Model;
using MedicalResultsTracker.Resources.Strings;
using MedicalResultsTracker.Services.Ai;
using MedicalResultsTracker.Services.Analysis;
using MedicalResultsTracker.Services.Database;
using MedicalResultsTracker.Services.Export;
using MedicalResultsTracker.Services.UI;

namespace MedicalResultsTracker.ViewModel
{
    /// <summary>Что показывать в списке показателей. Взаимоисключающие состояния, а не набор галочек.</summary>
    public enum TrendStatusFilter
    {
        /// <summary>Все показатели, включая те, что в норме.</summary>
        Any,

        /// <summary>Всё, что вышло за границы нормы, — в любую сторону.</summary>
        OutOfRange,

        High,

        Low,
    }

    /// <summary>
    /// Показатели с мини-графиками, разложенные по группам.
    ///
    /// Здесь же живёт отправка в ИИ-чат: это единственный экран, где видно все показатели
    /// сразу и где их можно отобрать. Отправляется ровно то, что осталось после фильтров, —
    /// иначе выбор ничего не значит и в чат каждый раз уезжает вся история.
    /// </summary>
    public partial class TrendsViewModel : BaseViewModel
    {
        /// <summary>Группа избранного всегда первая — за этими показателями следят намеренно.</summary>
        private static string FavoritesGroup => S.Trend_FavoritesGroup;

        private readonly IAnalysisService _analysis;
        private readonly IAnalyteCatalog _catalog;
        private readonly IExportService _export;
        private readonly IAiConsentService _consent;
        private readonly IAiAssistant _assistant;

        /// <summary>Ключи того, что сейчас видно. Именно они уезжают в чат.</summary>
        private readonly List<string> _visibleKeys = new();

        /// <summary>
        /// Всё, что прочитано из базы. Фильтры и поиск работают по этому списку, не трогая базу:
        /// иначе каждая буква в поиске уходила бы в чтение всей истории, а защёлка от повторного
        /// запуска глотала бы часть нажатий — и список отставал бы от строки поиска.
        /// </summary>
        private List<SeriesItemViewModel> _all = new();

        [ObservableProperty]
        private bool _isEmpty = true;

        /// <summary>Список пуст не потому, что данных нет, а потому, что их отсеяли фильтры.</summary>
        [ObservableProperty]
        private bool _isFilteredOut;

        [ObservableProperty]
        private string _search = string.Empty;

        [ObservableProperty]
        private string _hiddenSummary = string.Empty;

        [ObservableProperty]
        private bool _hasHidden;

        [ObservableProperty]
        private string _assistantStatus = string.Empty;

        private TrendStatusFilter _statusFilter = TrendStatusFilter.Any;
        private bool _onlyWithHistory = true;
        private bool _onlyFavorites;

        public TrendsViewModel(
            IAnalysisService analysis,
            IAnalyteCatalog catalog,
            IExportService export,
            IAiConsentService consent,
            IAiAssistant assistant)
        {
            _analysis = analysis;
            _catalog = catalog;
            _export = export;
            _consent = consent;
            _assistant = assistant;

            Title = S.Tab_Trends;

            Favorites = new FilterChipViewModel(S.Trend_OnlyFavorites, nameof(Favorites), _onlyFavorites);
            History = new FilterChipViewModel(S.Trend_OnlyWithHistory, nameof(History), _onlyWithHistory);

            Chips = new ObservableCollection<FilterChipViewModel>
            {
                Favorites,
                History,
                new(S.Trend_FilterOut, TrendStatusFilter.OutOfRange),
                new(S.Trend_FilterHigh, TrendStatusFilter.High),
                new(S.Trend_FilterLow, TrendStatusFilter.Low),
            };
        }

        public ObservableCollection<SeriesGroupViewModel> Groups { get; } = new();

        /// <summary>Фильтры одной лентой: два независимых переключателя и три состояния значения.</summary>
        public ObservableCollection<FilterChipViewModel> Chips { get; }

        private FilterChipViewModel Favorites { get; }

        private FilterChipViewModel History { get; }

        public override Task InitializeAsync() => RunAsync(LoadAsync, S.Err_Charts);

        [RelayCommand]
        private Task Refresh() => RunAsync(LoadAsync, S.Err_Charts);

        partial void OnSearchChanged(string value) => ApplyFilters();

        [RelayCommand]
        private Task Open(SeriesItemViewModel? item) => item is null
            ? Task.CompletedTask
            : Shell.Current.GoToAsync(
                $"{AppRoutes.TrendDetail}?{AppRoutes.SeriesKeyParameter}={Uri.EscapeDataString(item.Key)}");

        /// <summary>
        /// Нажатие на фильтр. Избранное и «с историей» переключаются сами по себе,
        /// а состояние значения — одно из трёх: включить «выше нормы», когда включено
        /// «ниже», значит выбрать «выше», а не оба сразу.
        /// </summary>
        [RelayCommand]
        private Task ToggleChip(FilterChipViewModel? chip)
        {
            if (chip is null)
            {
                return Task.CompletedTask;
            }

            if (chip.Parameter is TrendStatusFilter status)
            {
                _statusFilter = _statusFilter == status ? TrendStatusFilter.Any : status;

                foreach (FilterChipViewModel other in Chips)
                {
                    if (other.Parameter is TrendStatusFilter value)
                    {
                        other.IsActive = value == _statusFilter;
                    }
                }
            }
            else if (ReferenceEquals(chip, Favorites))
            {
                _onlyFavorites = chip.IsActive = !chip.IsActive;
            }
            else if (ReferenceEquals(chip, History))
            {
                _onlyWithHistory = chip.IsActive = !chip.IsActive;
            }

            ApplyFilters();

            return Task.CompletedTask;
        }

        /// <summary>Жест вправо: показатель попадает в избранное или уходит из него.</summary>
        [RelayCommand]
        private Task ToggleFavorite(SeriesItemViewModel? item) => item is null
            ? Task.CompletedTask
            : RunAsync(async () =>
            {
                await EnsureInCatalogAsync(item);
                await _catalog.SetFavoriteAsync(item.Key, !item.IsFavorite);

                await LoadAsync();
            }, S.Err_Charts);

        /// <summary>
        /// Жест влево: показатель уходит из списков. Измерения при этом остаются на месте —
        /// вернуть его можно в справочнике, о чём написано прямо под фильтрами.
        /// </summary>
        [RelayCommand]
        private Task Hide(SeriesItemViewModel? item) => item is null
            ? Task.CompletedTask
            : RunAsync(async () =>
            {
                await EnsureInCatalogAsync(item);
                await _catalog.SetHiddenAsync(item.Key, true);

                await LoadAsync();
            }, S.Err_Charts);

        /// <summary>
        /// Готовит таблицу текстом и открывает системный диалог «Поделиться».
        /// Приложение не выбирает получателя и никуда само не отправляет: в какое приложение
        /// уйдёт текст, решает пользователь в системном списке.
        /// </summary>
        [RelayCommand]
        private Task ShareForAi() => RunAsync(async () =>
        {
            if (_visibleKeys.Count == 0)
            {
                await Dialog.AlertAsync(S.Ai_Title, S.Ai_NothingSelected);
                return;
            }

            string text = await _export.BuildTextSummaryAsync(onlyKeys: _visibleKeys.ToList());

            await _export.ShareTextAsync(text, S.Ai_Title);
        }, S.Err_Text);

        /// <summary>
        /// Показатель мог никогда не попадать в справочник: строки заводятся и вручную.
        /// Без записи ни избранное, ни «скрыт» сохранить негде — заводим её по первому жесту.
        /// </summary>
        private async Task EnsureInCatalogAsync(SeriesItemViewModel item)
        {
            if (await _catalog.FindAsync(item.Key) is not null)
            {
                return;
            }

            await _catalog.SaveAsync(new Analyte
            {
                Code = item.Key,
                Name = item.Name,
                Unit = item.Unit,
                Category = AnalyteCategories.Own,
                IsBuiltIn = false,
            });
        }

        private async Task LoadAsync()
        {
            Title = S.Tab_Trends;

            IReadOnlyList<ParameterSeries> series = await _analysis.GetSeriesAsync();
            IReadOnlyList<Analyte> catalog = await _catalog.GetAllAsync();

            Dictionary<string, Analyte> byCode = catalog
                .GroupBy(a => a.Code, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            _all = series
                .Select(s => new SeriesItemViewModel(
                    s,
                    byCode.GetValueOrDefault(s.Key),
                    ToggleFavoriteCommand,
                    HideCommand))
                .ToList();

            AssistantStatus = _consent.Current.Scope == AiConsentScope.None
                ? S.Ai_Off
                : string.Format(S.Ai_On, _assistant.ProviderName);

            ApplyFilters();
        }

        /// <summary>Пересобирает список по текущим фильтрам. Ничего не читает — только раскладывает.</summary>
        private void ApplyFilters()
        {
            List<SeriesItemViewModel> items = _all
                .Where(item => !item.IsHidden)
                .Where(item => !_onlyWithHistory || item.HasTrend)
                .Where(item => !_onlyFavorites || item.IsFavorite)
                .Where(MatchesStatus)
                .Where(MatchesSearch)
                .ToList();

            _visibleKeys.Clear();
            _visibleKeys.AddRange(items.Select(i => i.Key));

            Groups.Clear();

            // Избранное дублируется в своей группе намеренно: оно должно быть под рукой,
            // не переставая при этом числиться в своей предметной группе.
            List<SeriesItemViewModel> favorites = items.Where(i => i.IsFavorite).ToList();

            if (favorites.Count > 0 && !_onlyFavorites)
            {
                Groups.Add(new SeriesGroupViewModel(FavoritesGroup, favorites.OrderBy(i => i.Name)));
            }

            foreach (IGrouping<string, SeriesItemViewModel> group in items
                .GroupBy(i => i.Category)
                .OrderBy(g => g.Key))
            {
                Groups.Add(new SeriesGroupViewModel(group.Key, group.OrderBy(i => i.Name)));
            }

            IsEmpty = _all.Count == 0;
            IsFilteredOut = _all.Count > 0 && items.Count == 0;

            // Скрытое не исчезает бесследно: счётчик под фильтрами говорит, что оно есть,
            // и где его искать. Иначе жест влево выглядит как потеря данных.
            int hidden = _all.Count(i => i.IsHidden);

            HasHidden = hidden > 0;
            HiddenSummary = hidden > 0 ? string.Format(S.Trend_HiddenCount, hidden) : string.Empty;
        }

        private bool MatchesStatus(SeriesItemViewModel item) => _statusFilter switch
        {
            TrendStatusFilter.OutOfRange => item.Status is ParameterStatus.Low or ParameterStatus.High,
            TrendStatusFilter.High => item.Status is ParameterStatus.High,
            TrendStatusFilter.Low => item.Status is ParameterStatus.Low,
            _ => true
        };

        private bool MatchesSearch(SeriesItemViewModel item)
        {
            string query = Search.Trim();

            return query.Length == 0 ||
                   item.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                   item.Key.Contains(query, StringComparison.OrdinalIgnoreCase);
        }
    }
}
