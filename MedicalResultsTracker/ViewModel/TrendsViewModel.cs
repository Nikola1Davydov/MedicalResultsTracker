using MedicalResultsTracker.Model;
using MedicalResultsTracker.Resources.Strings;
using MedicalResultsTracker.Services.Analysis;
using MedicalResultsTracker.Services.Database;

namespace MedicalResultsTracker.ViewModel
{
    /// <summary>Показатели с мини-графиками, разложенные по группам.</summary>
    public partial class TrendsViewModel : BaseViewModel
    {
        /// <summary>Группа избранного всегда первая — за этими показателями следят намеренно.</summary>
        private static string FavoritesGroup => S.Trend_FavoritesGroup;

        private readonly IAnalysisService _analysis;
        private readonly IAnalyteCatalog _catalog;

        [ObservableProperty]
        private bool _isEmpty = true;

        [ObservableProperty]
        private bool _onlyWithHistory = true;

        [ObservableProperty]
        private bool _onlyFavorites;

        public TrendsViewModel(IAnalysisService analysis, IAnalyteCatalog catalog)
        {
            _analysis = analysis;
            _catalog = catalog;

            Title = S.Tab_Trends;
        }

        public ObservableCollection<SeriesGroupViewModel> Groups { get; } = new();

        public override Task InitializeAsync() => RunAsync(LoadAsync, S.Err_Charts);

        [RelayCommand]
        private Task Refresh() => RunAsync(LoadAsync, S.Err_Charts);

        [RelayCommand]
        private Task Open(SeriesItemViewModel? item) => item is null
            ? Task.CompletedTask
            : Shell.Current.GoToAsync(
                $"{AppRoutes.TrendDetail}?{AppRoutes.SeriesKeyParameter}={Uri.EscapeDataString(item.Key)}");

        [RelayCommand]
        private Task ToggleHistoryFilter()
        {
            OnlyWithHistory = !OnlyWithHistory;

            return RunAsync(LoadAsync, S.Err_Charts);
        }

        [RelayCommand]
        private Task ToggleFavoritesFilter()
        {
            OnlyFavorites = !OnlyFavorites;

            return RunAsync(LoadAsync, S.Err_Charts);
        }

        private async Task LoadAsync()
        {
            IReadOnlyList<ParameterSeries> series = await _analysis.GetSeriesAsync();
            IReadOnlyList<Analyte> catalog = await _catalog.GetAllAsync();

            Dictionary<string, Analyte> byCode = catalog
                .GroupBy(a => a.Code, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            List<SeriesItemViewModel> items = series
                .Where(s => !OnlyWithHistory || s.HasTrend)
                .Select(s => new SeriesItemViewModel(s, byCode.GetValueOrDefault(s.Key)))
                .Where(item => !OnlyFavorites || item.IsFavorite)
                .ToList();

            Groups.Clear();

            // Избранное дублируется в своей группе намеренно: оно должно быть под рукой,
            // не переставая при этом числиться в своей предметной группе.
            List<SeriesItemViewModel> favorites = items.Where(i => i.IsFavorite).ToList();

            if (favorites.Count > 0 && !OnlyFavorites)
            {
                Groups.Add(new SeriesGroupViewModel(FavoritesGroup, favorites.OrderBy(i => i.Name)));
            }

            foreach (IGrouping<string, SeriesItemViewModel> group in items
                .GroupBy(i => i.Category)
                .OrderBy(g => g.Key))
            {
                Groups.Add(new SeriesGroupViewModel(group.Key, group.OrderBy(i => i.Name)));
            }

            IsEmpty = Groups.Count == 0;
        }
    }
}
