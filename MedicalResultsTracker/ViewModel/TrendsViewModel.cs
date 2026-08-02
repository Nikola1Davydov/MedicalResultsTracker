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

        /// <summary>Что сейчас наложено — одной строкой под поиском, чтобы это было видно без открытия списка.</summary>
        [ObservableProperty]
        private string _filterSummary = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FilterGlyph))]
        private bool _hasFilterSummary;

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
        }

        public ObservableCollection<SeriesGroupViewModel> Groups { get; } = new();

        /// <summary>
        /// Значок на кнопке: крестик, когда что-то наложено, иначе три полоски.
        /// </summary>
        public string FilterGlyph => HasFilterSummary ? "✕" : "≡";

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
        /// Одна кнопка на два действия, и какое сейчас — видно по значку.
        ///
        /// Крестик снимает всё одним нажатием: когда фильтр наложен, от него чаще всего
        /// хотят избавиться, и заставлять ради этого открывать список и искать в нём
        /// «Сбросить» — лишний шаг на ровном месте.
        ///
        /// Три полоски открывают список всех фильтров. Список растёт вниз и ничего не
        /// занимает на экране — новый фильтр можно будет просто дописать, а не искать
        /// ему место в ряду кнопок, который и так не помещался в ширину телефона.
        /// </summary>
        [RelayCommand]
        private Task Filter() => RunAsync(async () =>
        {
            if (HasFilterSummary)
            {
                await ResetAsync();
                return;
            }

            List<(string Label, Action Apply)> options = BuildFilterOptions();

            string? chosen = await Dialog.ChooseAsync(
                S.Trend_FilterTitle,
                options.Select(o => o.Label).ToArray());

            if (chosen is null)
            {
                return;
            }

            foreach ((string label, Action apply) in options)
            {
                if (label == chosen)
                {
                    apply();
                    break;
                }
            }

            ApplyFilters();
        }, S.Err_Charts);

        /// <summary>
        /// Обычный вид: фильтры сняты, скрытое возвращено. Скрытие делается тем же жестом,
        /// что и избранное, и человек считает его таким же фильтром — значит и снимается
        /// оно тем же крестиком, а не походом в справочник.
        /// </summary>
        private async Task ResetAsync()
        {
            _statusFilter = TrendStatusFilter.Any;
            _onlyFavorites = false;
            _onlyWithHistory = true;

            List<SeriesItemViewModel> hidden = _all.Where(i => i.IsHidden).ToList();

            foreach (SeriesItemViewModel item in hidden)
            {
                await _catalog.SetHiddenAsync(item.Key, false);
            }

            // Перечитываем, только если что-то изменилось в базе: фильтры снимаются на месте.
            if (hidden.Count > 0)
            {
                await LoadAsync();
                return;
            }

            ApplyFilters();
        }

        /// <summary>
        /// Пункты списка вместе с тем, что каждый делает. Список открывается только когда
        /// ничего не наложено, поэтому галочек в нём нет: каждый пункт что-то включает.
        /// </summary>
        private List<(string Label, Action Apply)> BuildFilterOptions() => new()
        {
            (S.Trend_FilterOut, () => _statusFilter = TrendStatusFilter.OutOfRange),
            (S.Trend_FilterHigh, () => _statusFilter = TrendStatusFilter.High),
            (S.Trend_FilterLow, () => _statusFilter = TrendStatusFilter.Low),
            (S.Trend_OnlyFavorites, () => _onlyFavorites = true),
            (S.Trend_AlsoSingle, () => _onlyWithHistory = false),
        };


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
        /// Жест влево: показатель уходит из списков. Измерения остаются на месте, а под поиском
        /// появляется «скрыто: N» — вернуть всё скрытое можно там же, в списке фильтров.
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
        /// Кладёт отобранное в буфер обмена — после того, как человек прочитал, что именно
        /// уйдёт. Приложение само никуда ничего не отправляет: текст просто оказывается
        /// в буфере, а вставить его или нет, решает человек уже в своём чате.
        /// </summary>
        [RelayCommand]
        private Task CopyForAi() => RunAsync(async () =>
        {
            if (_visibleKeys.Count == 0)
            {
                await Dialog.AlertAsync(S.Ai_Title, S.Ai_NothingSelected);
                return;
            }

            if (!await Dialog.ConfirmAsync(S.Ai_Title, S.Ai_Body, S.Ai_Copy))
            {
                return;
            }

            string text = await _export.BuildTextSummaryAsync(onlyKeys: _visibleKeys.ToList());

            await _export.CopyToClipboardAsync(text);

            await Dialog.AlertAsync(S.Ai_CopiedTitle, S.Ai_CopiedBody);
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

            FilterSummary = BuildFilterSummary();
            HasFilterSummary = FilterSummary.Length > 0;
        }

        /// <summary>
        /// Что наложено — словами. Скрытое считается наравне с фильтрами: жест влево убирает
        /// строку мгновенно, и без этой пометки это выглядело бы как потеря данных.
        /// </summary>
        private string BuildFilterSummary()
        {
            List<string> parts = new();

            if (_statusFilter != TrendStatusFilter.Any)
            {
                parts.Add(_statusFilter switch
                {
                    TrendStatusFilter.OutOfRange => S.Trend_FilterOut,
                    TrendStatusFilter.High => S.Trend_FilterHigh,
                    _ => S.Trend_FilterLow
                });
            }

            if (_onlyFavorites)
            {
                parts.Add(S.Trend_OnlyFavorites);
            }

            if (!_onlyWithHistory)
            {
                parts.Add(S.Trend_AlsoSingle);
            }

            int hidden = _all.Count(i => i.IsHidden);

            if (hidden > 0)
            {
                parts.Add(string.Format(S.Trend_HiddenCount, hidden));
            }

            return parts.Count == 0 ? string.Empty : string.Format(S.Trend_FilterSummary, string.Join(" · ", parts));
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
