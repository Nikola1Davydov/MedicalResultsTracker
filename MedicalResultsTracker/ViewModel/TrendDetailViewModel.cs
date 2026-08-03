using System.Globalization;
using MedicalResultsTracker.Resources.Strings;
using MedicalResultsTracker.Controls;
using MedicalResultsTracker.Model;
using MedicalResultsTracker.Services.Analysis;
using MedicalResultsTracker.Services.Database;
using MedicalResultsTracker.Services.UI;

namespace MedicalResultsTracker.ViewModel
{
    /// <summary>Большой график одного показателя плюс таблица значений под ним.</summary>
    public partial class TrendDetailViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IAnalysisService _analysis;
        private readonly IAnalyteCatalog _catalog;
        private readonly IBloodTestRepository _repository;

        [ObservableProperty]
        private string _subtitle = string.Empty;

        [ObservableProperty]
        private string _rangeText = string.Empty;

        [ObservableProperty]
        private TrendChartDrawable _chart = new();

        [ObservableProperty]
        private bool _isEmpty = true;

        [ObservableProperty]
        private bool _hasData;

        /// <summary>Измерения сделаны в разных единицах — сравнивать их нельзя, и это надо сказать.</summary>
        [ObservableProperty]
        private bool _hasMixedUnits;

        private string? _key;

        /// <summary>Прочитанный ряд: из него берутся текущие границы для редактора.</summary>
        private ParameterSeries? _series;

        /// <summary>В списке похожих выбрали «показать все»: за первым списком идёт второй.</summary>
        private bool _fullListRequested;

        /// <summary>Открыт ли редактор нормы. Поля показываются на месте, без отдельного экрана.</summary>
        [ObservableProperty]
        private bool _isEditingRange;

        [ObservableProperty]
        private string _refMinText = string.Empty;

        [ObservableProperty]
        private string _refMaxText = string.Empty;

        public TrendDetailViewModel(
            IAnalysisService analysis,
            IAnalyteCatalog catalog,
            IBloodTestRepository repository)
        {
            _analysis = analysis;
            _catalog = catalog;
            _repository = repository;

            Title = S.Csv_Parameter;
        }

        public ObservableCollection<SeriesRowViewModel> Values { get; } = new();

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue(AppRoutes.SeriesKeyParameter, out object? value))
            {
                _key = Uri.UnescapeDataString(Convert.ToString(value) ?? string.Empty);
            }
        }

        public override Task InitializeAsync() => RunAsync(LoadAsync, S.Err_Chart);

        /// <summary>
        /// Открывает поля нормы, подставив то, что записано сейчас.
        ///
        /// Норму приходится вписывать руками чаще, чем хотелось бы: чат-бот вытаскивает её
        /// из бланка не всегда, а без неё приложение не может сказать ровно то, ради чего
        /// его и завели, — вышло значение за пределы или нет.
        /// </summary>
        [RelayCommand]
        private void EditRange()
        {
            RefMinText = Format(_series?.RefMin);
            RefMaxText = Format(_series?.RefMax);
            IsEditingRange = true;
        }

        [RelayCommand]
        private void CancelRange() => IsEditingRange = false;

        /// <summary>
        /// Записывает норму во все измерения показателя и в справочник.
        ///
        /// Во все — потому что «норма показателя» человеком мыслится как одна на весь ряд:
        /// иначе на графике полоса прыгала бы от точки к точке. Границы из бланка при этом
        /// перезаписываются, и об этом прямо сказано в подтверждении.
        /// </summary>
        [RelayCommand]
        private Task SaveRange() => RunAsync(async () =>
        {
            if (string.IsNullOrEmpty(_key))
            {
                return;
            }

            double? min = LabNumber.Parse(RefMinText);
            double? max = LabNumber.Parse(RefMaxText);

            IReadOnlyDictionary<string, int> usage = await _repository.GetUsageByCodeAsync();

            usage.TryGetValue(_key, out int count);

            bool clearing = min is null && max is null;

            bool confirmed = await Dialog.ConfirmAsync(
                clearing ? S.Trend_RangeClearTitle : S.Trend_RangeSetTitle,
                string.Format(clearing ? S.Trend_RangeClearBody : S.Trend_RangeSetBody, count),
                clearing ? S.Common_Delete : S.Common_Save);

            if (!confirmed)
            {
                return;
            }

            await _repository.SetRangeAsync(_key, min, max);
            await RememberRangeAsync(min, max);

            IsEditingRange = false;

            await LoadAsync();
        }, S.Err_RangeSave);

        /// <summary>
        /// Та же норма уходит в справочник — чтобы в следующий раз она подставилась сама.
        /// Правленая встроенная запись помечается: обновление набора её больше не трогает.
        /// </summary>
        private async Task RememberRangeAsync(double? min, double? max)
        {
            if (_key is null || await _catalog.FindAsync(_key) is not Analyte known)
            {
                return;
            }

            known.RefMin = min;
            known.RefMax = max;
            known.IsCustomized = known.IsBuiltIn;

            await _catalog.SaveAsync(known);
        }

        private static string Format(double? value) =>
            value?.ToString("0.####", CultureInfo.CurrentCulture) ?? string.Empty;

        /// <summary>
        /// «Это тот же показатель, что и вот этот» — прямо здесь, на экране значения.
        ///
        /// Раньше то же самое делалось только из справочника в настройках, и найти его там
        /// было невозможно: человек смотрит на разъехавшийся показатель здесь, а чинить
        /// его отправляли в другой конец приложения.
        ///
        /// Похожие названия предлагаются первыми, но список не ограничен ими: чат-бот
        /// способен написать название так, что ни одно правило схожести его не поймает.
        /// </summary>
        [RelayCommand]
        private Task Merge() => RunAsync(async () =>
        {
            if (string.IsNullOrEmpty(_key))
            {
                return;
            }

            List<Analyte> others = (await _catalog.GetAllAsync())
                .Where(a => !a.IsHidden && !string.Equals(a.Code, _key, StringComparison.OrdinalIgnoreCase))
                .Where(a => !string.IsNullOrWhiteSpace(a.Name))
                .ToList();

            if (others.Count == 0)
            {
                await Dialog.AlertAsync(S.CatEdit_MergePickTitle, S.CatEdit_MergePickBody);
                return;
            }

            Analyte? target = await PickTargetAsync(others);

            if (target is null)
            {
                return;
            }

            IReadOnlyDictionary<string, int> usage = await _repository.GetUsageByCodeAsync();

            usage.TryGetValue(_key, out int count);

            bool confirmed = await Dialog.ConfirmAsync(
                S.CatEdit_MergeConfirmTitle,
                string.Format(S.CatEdit_MergeConfirmBody, Title, count, target.Name),
                S.CatEdit_Merge);

            if (!confirmed)
            {
                return;
            }

            int moved = await _repository.ReassignCodeAsync(_key, target.Code);

            await ForgetSourceAsync();

            await Dialog.AlertAsync(S.CatEdit_MergeDoneTitle, string.Format(S.CatEdit_MergeDoneBody, moved));

            // Показателя с этим ключом больше нет — оставаться на его экране незачем.
            await Shell.Current.GoToAsync("..");
        }, S.Err_Merge);

        /// <summary>
        /// Сначала похожие названия, потом — по желанию — весь справочник. Вываливать сорок
        /// записей сразу бессмысленно: в девяти случаях из десяти нужное лежит в первых трёх.
        /// </summary>
        private async Task<Analyte?> PickTargetAsync(List<Analyte> others)
        {
            IReadOnlyList<Analyte> similar = NameMatch.Candidates(Title, others, limit: 5);

            if (similar.Count > 0)
            {
                Analyte? chosen = await ChooseAsync(similar, withFullList: true);

                if (chosen is not null || !_fullListRequested)
                {
                    return chosen;
                }

                _fullListRequested = false;
            }

            return await ChooseAsync(others.OrderBy(a => a.Name, StringComparer.CurrentCulture).ToList(), withFullList: false);
        }

        private async Task<Analyte?> ChooseAsync(IReadOnlyList<Analyte> items, bool withFullList)
        {
            Dictionary<string, Analyte> byLabel = new(StringComparer.Ordinal);

            foreach (Analyte item in items)
            {
                string label = string.IsNullOrWhiteSpace(item.Unit)
                    ? item.Name
                    : string.Format(S.Match_Option, item.Name, item.Unit);

                byLabel.TryAdd(label, item);
            }

            List<string> options = byLabel.Keys.ToList();

            if (withFullList)
            {
                options.Add(S.Trend_MergeAll);
            }

            string? chosen = await Dialog.ChooseAsync(string.Format(S.Match_Title, Title), options.ToArray());

            _fullListRequested = chosen == S.Trend_MergeAll;

            return chosen is null ? null : byLabel.GetValueOrDefault(chosen);
        }

        /// <summary>
        /// Запись, из которой всё перенесли, больше ничего не хранит. Встроенную прячем —
        /// удалённая вернулась бы при следующем запуске из встроенного набора.
        /// </summary>
        private async Task ForgetSourceAsync()
        {
            if (_key is null || await _catalog.FindAsync(_key) is not Analyte source)
            {
                return;
            }

            if (source.IsBuiltIn)
            {
                source.IsHidden = true;

                await _catalog.SaveAsync(source);
                return;
            }

            await _catalog.DeleteAsync(_key);
        }

        private async Task LoadAsync()
        {
            Values.Clear();

            if (string.IsNullOrEmpty(_key))
            {
                IsEmpty = true;
                HasData = false;
                return;
            }

            ParameterSeries? series = await _analysis.GetSeriesAsync(_key);

            _series = series;

            if (series is null)
            {
                IsEmpty = true;
                Subtitle = S.Trend_NoData;
                return;
            }

            Title = series.Name;
            Chart = new TrendChartDrawable { Series = series };
            IsEmpty = false;
            HasData = true;

            ReferenceRange range = new() { Min = series.RefMin, Max = series.RefMax };

            RangeText = range.IsDefined
                ? string.Format(S.Trend_RefKnown, range, series.Unit).Trim()
                : S.Trend_RefUnknown;
            HasMixedUnits = series.HasMixedUnits;

            Subtitle = series.Points.Count == 1
                ? S.Trend_SinglePoint
                : string.Format(S.Trend_Since, series.Points.Count, series.Points[0].Date.ToString("d", CultureInfo.CurrentCulture));

            // Свежие значения сверху — так удобнее сверяться с последним бланком.
            for (int i = series.Points.Count - 1; i >= 0; i--)
            {
                SeriesPoint point = series.Points[i];
                double? previous = i > 0 ? series.Points[i - 1].Value : null;

                Values.Add(new SeriesRowViewModel(point, previous, series.Unit));
            }
        }
    }

    /// <summary>Строка таблицы значений под графиком.</summary>
    public sealed class SeriesRowViewModel
    {
        public SeriesRowViewModel(SeriesPoint point, double? previous, string? unit)
        {
            DateText = point.Date.ToString("d", CultureInfo.CurrentCulture);
            ValueText = $"{point.Value.ToString("0.####", CultureInfo.CurrentCulture)} {unit}".Trim();
            StatusText = StatusPalette.Describe(point.Status);
            StatusColor = StatusPalette.For(point.Status);

            DeltaText = previous is double before
                ? (point.Value - before) switch
                {
                    > 0 => $"↑ {(point.Value - before).ToString("0.####", CultureInfo.CurrentCulture)}",
                    < 0 => $"↓ {Math.Abs(point.Value - before).ToString("0.####", CultureInfo.CurrentCulture)}",
                    _ => "→"
                }
                : string.Empty;
        }

        public string DateText { get; }

        public string ValueText { get; }

        public string DeltaText { get; }

        public string StatusText { get; }

        public Color StatusColor { get; }
    }
}
