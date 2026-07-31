using MedicalResultsTracker.Model;
using MedicalResultsTracker.Resources.Strings;
using MedicalResultsTracker.Services.Database;
using MedicalResultsTracker.Services.UI;

namespace MedicalResultsTracker.ViewModel
{
    /// <summary>Составление своего набора показателей: имя и отметки в списке справочника.</summary>
    public partial class ViewEditViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IMatrixViewRepository _views;
        private readonly IAnalyteCatalog _catalog;
        private readonly MatrixState _matrix;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _query = string.Empty;

        [ObservableProperty]
        private bool _isExisting;

        [ObservableProperty]
        private string _selectionSummary = string.Empty;

        private Guid _viewId = Guid.NewGuid();
        private IReadOnlyList<Analyte> _all = Array.Empty<Analyte>();

        /// <summary>Порядок выбора сохраняется: пользователь ставит галочки не случайно.</summary>
        private readonly List<string> _selected = new();

        public ViewEditViewModel(IMatrixViewRepository views, IAnalyteCatalog catalog, MatrixState matrix)
        {
            _views = views;
            _catalog = catalog;
            _matrix = matrix;

            Title = S.ViewEdit_TitleNew;
        }

        public ObservableCollection<ViewItemViewModel> Items { get; } = new();

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue(AppRoutes.ViewIdParameter, out object? value) &&
                Guid.TryParse(Convert.ToString(value), out Guid id))
            {
                _viewId = id;
                IsExisting = true;
            }
        }

        public override Task InitializeAsync() => RunAsync(LoadAsync, S.Err_View);

        partial void OnQueryChanged(string value) => ApplyFilter();

        [RelayCommand]
        private void Toggle(ViewItemViewModel? item)
        {
            if (item is null)
            {
                return;
            }

            item.IsSelected = !item.IsSelected;

            if (item.IsSelected)
            {
                _selected.Add(item.Code);
            }
            else
            {
                _selected.Remove(item.Code);
            }

            UpdateSummary();
        }

        [RelayCommand]
        private Task Save() => RunAsync(async () =>
        {
            string name = Name.Trim();

            if (name.Length == 0)
            {
                await Dialog.AlertAsync(S.ViewEdit_NoNameTitle, S.ViewEdit_NoNameBody);
                return;
            }

            if (_selected.Count == 0)
            {
                await Dialog.AlertAsync(S.ViewEdit_EmptyTitle, S.ViewEdit_EmptyBody);
                return;
            }

            await _views.SaveAsync(new MatrixView
            {
                Id = _viewId,
                Name = name,
                Codes = _selected.ToList(),
            });

            _matrix.PendingViewId = _viewId;

            await Shell.Current.GoToAsync("..");
        }, S.Err_ViewSave);

        [RelayCommand]
        private Task Delete() => RunAsync(async () =>
        {
            if (!IsExisting)
            {
                await Shell.Current.GoToAsync("..");
                return;
            }

            if (!await Dialog.ConfirmAsync(S.ViewEdit_DeleteTitle, S.ViewEdit_DeleteBody, S.Common_Delete))
            {
                return;
            }

            await _views.DeleteAsync(_viewId);
            await Shell.Current.GoToAsync("..");
        }, S.Err_ViewDelete);

        [RelayCommand]
        private Task Cancel() => Shell.Current.GoToAsync("..");

        private async Task LoadAsync()
        {
            _all = await _catalog.GetAllAsync();

            if (IsExisting && await _views.GetAsync(_viewId) is MatrixView existing)
            {
                Title = existing.Name;
                Name = existing.Name;

                _selected.Clear();
                _selected.AddRange(existing.Codes);
            }
            else
            {
                Title = S.ViewEdit_TitleNew;
            }

            ApplyFilter();
            UpdateSummary();
        }

        private void ApplyFilter()
        {
            string trimmed = Query.Trim();

            Items.Clear();

            foreach (Analyte analyte in _all
                .Where(a => !a.IsHidden)
                .Where(a => trimmed.Length == 0
                            || a.Name.Contains(trimmed, StringComparison.CurrentCultureIgnoreCase))
                .OrderByDescending(a => _selected.Contains(a.Code))
                .ThenBy(a => a.Name, StringComparer.CurrentCulture))
            {
                Items.Add(new ViewItemViewModel
                {
                    Code = analyte.Code,
                    Name = analyte.Name,
                    Group = AnalyteDisplay.Category(analyte.Category),
                    IsSelected = _selected.Contains(analyte.Code),
                });
            }
        }

        private void UpdateSummary() => SelectionSummary = _selected.Count switch
        {
            0 => S.ViewEdit_NothingPicked,
            1 => S.ViewEdit_OnePicked,
            _ => string.Format(S.ViewEdit_ManyPicked, _selected.Count)
        };
    }

    /// <summary>Строка выбора показателя в набор.</summary>
    public partial class ViewItemViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isSelected;

        public required string Code { get; init; }

        public required string Name { get; init; }

        public required string Group { get; init; }
    }
}
