using MedicalResultsTracker.Model;
using MedicalResultsTracker.Resources.Strings;
using MedicalResultsTracker.Services.Database;

namespace MedicalResultsTracker.ViewModel
{
    /// <summary>Справочник показателей: что подставляется при вводе анализа и как всё это сгруппировано.</summary>
    public partial class CatalogViewModel : BaseViewModel
    {
        private readonly IAnalyteCatalog _catalog;
        private readonly IBloodTestRepository _repository;

        [ObservableProperty]
        private string _query = string.Empty;

        [ObservableProperty]
        private bool _showHidden;

        [ObservableProperty]
        private bool _onlyFavorites;

        [ObservableProperty]
        private string _summary = string.Empty;

        private IReadOnlyList<Analyte> _all = Array.Empty<Analyte>();
        private IReadOnlyDictionary<string, int> _usage = new Dictionary<string, int>();

        public CatalogViewModel(IAnalyteCatalog catalog, IBloodTestRepository repository)
        {
            _catalog = catalog;
            _repository = repository;

            Title = S.Cat_Title;
        }

        public ObservableCollection<CatalogGroupViewModel> Groups { get; } = new();

        public override Task InitializeAsync() => RunAsync(LoadAsync, S.Err_Catalog);

        partial void OnQueryChanged(string value) => ApplyFilter();

        partial void OnShowHiddenChanged(bool value) => ApplyFilter();

        partial void OnOnlyFavoritesChanged(bool value) => ApplyFilter();

        [RelayCommand]
        private Task Add() => Shell.Current.GoToAsync(AppRoutes.CatalogEdit);

        [RelayCommand]
        private Task Open(CatalogItemViewModel? item) => item is null
            ? Task.CompletedTask
            : Shell.Current.GoToAsync(
                $"{AppRoutes.CatalogEdit}?{AppRoutes.AnalyteCodeParameter}={Uri.EscapeDataString(item.Code)}");

        /// <summary>Звёздочка прямо в списке: отметить десяток показателей через карточку — долго.</summary>
        [RelayCommand]
        private Task ToggleFavorite(CatalogItemViewModel? item) => item is null
            ? Task.CompletedTask
            : RunAsync(async () =>
            {
                await _catalog.SetFavoriteAsync(item.Code, !item.IsFavorite);
                await LoadAsync();
            }, S.Err_Favorite);

        private async Task LoadAsync()
        {
            _all = await _catalog.GetAllAsync();
            _usage = await _repository.GetUsageByCodeAsync();

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string trimmed = Query.Trim();

            List<Analyte> filtered = _all
                .Where(a => ShowHidden || !a.IsHidden)
                .Where(a => !OnlyFavorites || a.IsFavorite)
                .Where(a => trimmed.Length == 0
                            || a.Name.Contains(trimmed, StringComparison.CurrentCultureIgnoreCase)
                            || a.Code.Contains(trimmed, StringComparison.OrdinalIgnoreCase))
                .ToList();

            Groups.Clear();

            foreach (IGrouping<string, Analyte> group in filtered
                .GroupBy(a => AnalyteDisplay.Category(a.Category))
                .OrderBy(g => g.Key == S.Trend_NoGroup)
                .ThenBy(g => g.Key))
            {
                List<CatalogItemViewModel> items = group
                    .OrderByDescending(a => a.IsFavorite)
                    .ThenBy(a => a.SortOrder)
                    .ThenBy(a => a.Name)
                    .Select(a => new CatalogItemViewModel(a, _usage.GetValueOrDefault(a.Code)))
                    .ToList();

                Groups.Add(new CatalogGroupViewModel(group.Key, items));
            }

            int hidden = _all.Count(a => a.IsHidden);
            int favorites = _all.Count(a => a.IsFavorite);

            Summary = hidden == 0
                ? string.Format(S.Cat_Summary, _all.Count, favorites)
                : string.Format(S.Cat_SummaryHidden, _all.Count, favorites, hidden);
        }
    }

    /// <summary>Строка справочника.</summary>
    public sealed class CatalogItemViewModel
    {
        public CatalogItemViewModel(Analyte analyte, int measurements)
        {
            Code = analyte.Code;
            Name = analyte.Name;
            IsHidden = analyte.IsHidden;
            IsFavorite = analyte.IsFavorite;
            FavoriteGlyph = analyte.IsFavorite ? "★" : "☆";

            string range = analyte.DefaultRange.IsDefined ? analyte.DefaultRange.ToString() : S.Cat_NoRef;
            string used = measurements switch
            {
                0 => S.Cat_Unused,
                1 => S.Cat_OneMeasurement,
                _ => string.Format(S.Cat_ManyMeasurements, measurements)
            };

            string origin = analyte.IsBuiltIn ? S.Cat_BuiltIn : S.Cat_Own;

            Subtitle = string.Join(" · ", new[] { analyte.Unit, range, used, origin }
                .Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        public string Code { get; }

        public string Name { get; }

        public string Subtitle { get; }

        public bool IsHidden { get; }

        public bool IsFavorite { get; }

        public string FavoriteGlyph { get; }
    }

    /// <summary>Группа справочника. Наследник List — этого требует сгруппированный CollectionView.</summary>
    public sealed class CatalogGroupViewModel : List<CatalogItemViewModel>
    {
        public CatalogGroupViewModel(string name, IEnumerable<CatalogItemViewModel> items)
            : base(items)
        {
            Name = name;
        }

        public string Name { get; }

        public string Subtitle => Count == 1 ? S.Cat_OneParam : string.Format(S.Cat_ManyParams, Count);
    }
}
