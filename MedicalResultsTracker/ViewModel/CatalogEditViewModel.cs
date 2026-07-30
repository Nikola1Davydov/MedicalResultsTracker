using System.Globalization;
using MedicalResultsTracker.Model;
using MedicalResultsTracker.Services.Database;
using MedicalResultsTracker.Services.UI;

namespace MedicalResultsTracker.ViewModel
{
    /// <summary>Правка одной записи справочника: название, единицы, типовая норма, объединение дублей.</summary>
    public partial class CatalogEditViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IAnalyteCatalog _catalog;
        private readonly IBloodTestRepository _repository;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string? _unit;

        [ObservableProperty]
        private string? _category;

        [ObservableProperty]
        private string _refMinText = string.Empty;

        [ObservableProperty]
        private string _refMaxText = string.Empty;

        [ObservableProperty]
        private string? _notes;

        [ObservableProperty]
        private bool _isHidden;

        [ObservableProperty]
        private bool _isFavorite;

        /// <summary>Выбор из уже существующих групп. Пишет в <see cref="Category"/>, где можно ввести и новую.</summary>
        [ObservableProperty]
        private string? _selectedCategory;

        [ObservableProperty]
        private bool _isExisting;

        [ObservableProperty]
        private bool _isBuiltIn;

        [ObservableProperty]
        private string _codeText = string.Empty;

        [ObservableProperty]
        private string _usageText = string.Empty;

        [ObservableProperty]
        private Analyte? _mergeTarget;

        private string? _code;
        private int _usageCount;

        public CatalogEditViewModel(IAnalyteCatalog catalog, IBloodTestRepository repository)
        {
            _catalog = catalog;
            _repository = repository;

            Title = "Показатель";
        }

        /// <summary>Куда можно объединить эту запись — все остальные записи справочника.</summary>
        public ObservableCollection<Analyte> MergeTargets { get; } = new();

        /// <summary>Уже заведённые группы — чтобы не плодить «Липиды» и «липиды» рядом.</summary>
        public ObservableCollection<string> Categories { get; } = new();

        partial void OnSelectedCategoryChanged(string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                Category = value;
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue(AppRoutes.AnalyteCodeParameter, out object? value))
            {
                _code = Uri.UnescapeDataString(Convert.ToString(value) ?? string.Empty);
                IsExisting = !string.IsNullOrEmpty(_code);
            }
        }

        public override Task InitializeAsync() => RunAsync(LoadAsync, "Не удалось открыть показатель");

        [RelayCommand]
        private Task Save() => RunAsync(async () =>
        {
            string name = Name.Trim();

            if (name.Length == 0)
            {
                await Dialog.AlertAsync("Не хватает названия", "Укажите название показателя.");
                return;
            }

            string code = _code ?? AnalyteCode.FromName(name);

            if (!IsExisting && await _catalog.FindAsync(code) is not null)
            {
                await Dialog.AlertAsync(
                    "Такой показатель уже есть",
                    "Показатель с таким названием уже в справочнике. Откройте его и отредактируйте.");
                return;
            }

            await _catalog.SaveAsync(new Analyte
            {
                Code = code,
                Name = name,
                Unit = string.IsNullOrWhiteSpace(Unit) ? null : Unit.Trim(),
                Category = string.IsNullOrWhiteSpace(Category) ? "Мои показатели" : Category.Trim(),
                RefMin = ParameterRowViewModel.ParseNumber(RefMinText),
                RefMax = ParameterRowViewModel.ParseNumber(RefMaxText),
                Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim(),
                IsBuiltIn = IsBuiltIn,
                IsHidden = IsHidden,
                IsFavorite = IsFavorite,
            });

            await Shell.Current.GoToAsync("..");
        }, "Не удалось сохранить показатель");

        [RelayCommand]
        private Task Delete() => RunAsync(async () =>
        {
            if (!IsExisting || _code is null)
            {
                await Shell.Current.GoToAsync("..");
                return;
            }

            if (IsBuiltIn)
            {
                await Dialog.AlertAsync(
                    "Встроенный показатель",
                    "Встроенные показатели не удаляются — при следующем запуске они вернутся. " +
                    "Уберите его из подсказок переключателем «Скрыть».");
                return;
            }

            string message = _usageCount == 0
                ? "Запись справочника будет удалена."
                : $"Запись справочника будет удалена. Сохранённые измерения ({_usageCount}) останутся на месте " +
                  "и продолжат собираться в один график — справочник лишь подставляет значения при вводе.";

            if (!await Dialog.ConfirmAsync("Удалить из справочника?", message, "Удалить"))
            {
                return;
            }

            await _catalog.DeleteAsync(_code);
            await Shell.Current.GoToAsync("..");
        }, "Не удалось удалить показатель");

        /// <summary>
        /// Объединяет дубль: все измерения этой записи переезжают на выбранный показатель,
        /// после чего запись исчезает из справочника (встроенная — прячется).
        /// </summary>
        [RelayCommand]
        private Task Merge() => RunAsync(async () =>
        {
            if (_code is null || MergeTarget is null)
            {
                await Dialog.AlertAsync("Выберите показатель", "Укажите, с каким показателем объединить этот.");
                return;
            }

            bool confirmed = await Dialog.ConfirmAsync(
                "Объединить показатели?",
                $"Измерения «{Name}» ({_usageCount}) перейдут к «{MergeTarget.Name}» и дальше будут одной линией на графике. " +
                "Отменить это можно только повторным объединением в обратную сторону.",
                "Объединить");

            if (!confirmed)
            {
                return;
            }

            int moved = await _repository.ReassignCodeAsync(_code, MergeTarget.Code);

            if (IsBuiltIn)
            {
                await _catalog.SaveAsync(new Analyte
                {
                    Code = _code,
                    Name = Name,
                    Unit = Unit,
                    Category = Category,
                    RefMin = ParameterRowViewModel.ParseNumber(RefMinText),
                    RefMax = ParameterRowViewModel.ParseNumber(RefMaxText),
                    Notes = Notes,
                    IsBuiltIn = true,
                    IsHidden = true,
                    IsFavorite = false,
                });
            }
            else
            {
                await _catalog.DeleteAsync(_code);
            }

            await Dialog.AlertAsync("Готово", $"Перенесено измерений: {moved}.");
            await Shell.Current.GoToAsync("..");
        }, "Не удалось объединить показатели");

        [RelayCommand]
        private Task Cancel() => Shell.Current.GoToAsync("..");

        private async Task LoadAsync()
        {
            IReadOnlyList<Analyte> all = await _catalog.GetAllAsync();

            MergeTargets.Clear();

            foreach (Analyte analyte in all.Where(a => a.Code != _code).OrderBy(a => a.Name))
            {
                MergeTargets.Add(analyte);
            }

            Categories.Clear();

            foreach (string category in await _catalog.GetCategoriesAsync())
            {
                Categories.Add(category);
            }

            if (!IsExisting || _code is null)
            {
                Title = "Новый показатель";
                CodeText = "Код будет создан из названия";
                UsageText = string.Empty;
                return;
            }

            Analyte? current = await _catalog.FindAsync(_code);

            if (current is null)
            {
                IsExisting = false;
                return;
            }

            Name = current.Name;
            Unit = current.Unit;
            Category = current.Category;
            RefMinText = Format(current.RefMin);
            RefMaxText = Format(current.RefMax);
            Notes = current.Notes;
            IsHidden = current.IsHidden;
            IsFavorite = current.IsFavorite;
            IsBuiltIn = current.IsBuiltIn;
            CodeText = $"Код: {current.Code}";
            Title = current.Name;

            IReadOnlyDictionary<string, int> usage = await _repository.GetUsageByCodeAsync();

            usage.TryGetValue(current.Code, out _usageCount);

            UsageText = _usageCount switch
            {
                0 => "Измерений с этим показателем пока нет.",
                1 => "1 сохранённое измерение.",
                _ => $"{_usageCount} сохранённых измерений."
            };
        }

        private static string Format(double? value) =>
            value?.ToString("0.####", CultureInfo.CurrentCulture) ?? string.Empty;
    }
}
