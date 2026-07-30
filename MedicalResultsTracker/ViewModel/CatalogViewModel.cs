using MedicalResultsTracker.Model;
using MedicalResultsTracker.Services.Database;

namespace MedicalResultsTracker.ViewModel
{
    /// <summary>Справочник показателей: что подставляется при вводе анализа.</summary>
    public partial class CatalogViewModel : BaseViewModel
    {
        private readonly IAnalyteCatalog _catalog;
        private readonly IBloodTestRepository _repository;

        [ObservableProperty]
        private string _query = string.Empty;

        [ObservableProperty]
        private bool _showHidden;

        [ObservableProperty]
        private string _summary = string.Empty;

        private IReadOnlyList<Analyte> _all = Array.Empty<Analyte>();
        private IReadOnlyDictionary<string, int> _usage = new Dictionary<string, int>();

        public CatalogViewModel(IAnalyteCatalog catalog, IBloodTestRepository repository)
        {
            _catalog = catalog;
            _repository = repository;

            Title = "Справочник";
        }

        public ObservableCollection<CatalogItemViewModel> Items { get; } = new();

        public override Task InitializeAsync() => RunAsync(LoadAsync, "Не удалось открыть справочник");

        partial void OnQueryChanged(string value) => ApplyFilter();

        partial void OnShowHiddenChanged(bool value) => ApplyFilter();

        [RelayCommand]
        private Task Add() => Shell.Current.GoToAsync(AppRoutes.CatalogEdit);

        [RelayCommand]
        private Task Open(CatalogItemViewModel? item) => item is null
            ? Task.CompletedTask
            : Shell.Current.GoToAsync(
                $"{AppRoutes.CatalogEdit}?{AppRoutes.AnalyteCodeParameter}={Uri.EscapeDataString(item.Code)}");

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
                .Where(a => trimmed.Length == 0
                            || a.Name.Contains(trimmed, StringComparison.CurrentCultureIgnoreCase)
                            || a.Code.Contains(trimmed, StringComparison.OrdinalIgnoreCase))
                .ToList();

            Items.Clear();

            foreach (Analyte analyte in filtered)
            {
                _usage.TryGetValue(analyte.Code, out int used);

                Items.Add(new CatalogItemViewModel(analyte, used));
            }

            int hidden = _all.Count(a => a.IsHidden);

            Summary = hidden == 0
                ? $"{_all.Count} показателей"
                : $"{_all.Count} показателей, из них скрыто {hidden}";
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

            string range = analyte.DefaultRange.IsDefined ? analyte.DefaultRange.ToString() : "норма не задана";
            string used = measurements switch
            {
                0 => "не использовался",
                1 => "1 измерение",
                _ => $"{measurements} измерений"
            };

            string origin = analyte.IsBuiltIn ? "встроенный" : "свой";

            Subtitle = string.Join(" · ", new[] { analyte.Unit, range, used, origin }
                .Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        public string Code { get; }

        public string Name { get; }

        public string Subtitle { get; }

        public bool IsHidden { get; }
    }
}
