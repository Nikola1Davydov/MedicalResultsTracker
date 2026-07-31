using MedicalResultsTracker.Controls;
using MedicalResultsTracker.Resources.Strings;
using MedicalResultsTracker.Model;
using MedicalResultsTracker.Services.Database;
using MedicalResultsTracker.Services.Export;

namespace MedicalResultsTracker.ViewModel
{
    /// <summary>Список всех сохранённых анализов.</summary>
    public partial class HistoryViewModel : BaseViewModel
    {
        private readonly IBloodTestRepository _repository;
        private readonly IExportService _export;

        [ObservableProperty]
        private bool _isEmpty = true;

        public HistoryViewModel(IBloodTestRepository repository, IExportService export)
        {
            _repository = repository;
            _export = export;

            Title = S.Tab_History;
        }

        public ObservableCollection<TestListItemViewModel> Tests { get; } = new();

        public override Task InitializeAsync() => RunAsync(LoadAsync, S.Err_History);

        [RelayCommand]
        private Task Refresh() => RunAsync(LoadAsync, S.Err_History);

        [RelayCommand]
        private Task Add() => Shell.Current.GoToAsync(AppRoutes.TestEdit);

        [RelayCommand]
        private Task Open(TestListItemViewModel? item) => item is null
            ? Task.CompletedTask
            : Shell.Current.GoToAsync($"{AppRoutes.TestEdit}?{AppRoutes.TestIdParameter}={item.Id}");

        [RelayCommand]
        private Task Export() => RunAsync(async () =>
        {
            string path = await _export.ExportMatrixCsvAsync();
            await _export.ShareAsync(path, S.Share_Results);
        }, S.Err_Export);

        private async Task LoadAsync()
        {
            IReadOnlyList<BloodTest> tests = await _repository.GetAllAsync();

            Tests.Clear();

            foreach (BloodTest test in tests)
            {
                Tests.Add(new TestListItemViewModel(test));
            }

            IsEmpty = Tests.Count == 0;
        }
    }

    /// <summary>Карточка анализа в списке истории.</summary>
    public sealed class TestListItemViewModel
    {
        public TestListItemViewModel(BloodTest test)
        {
            Id = test.Id;
            Title = test.Title;
            ParameterCount = test.Parameters.Count;
            OutOfRangeCount = test.OutOfRangeCount;
        }

        public Guid Id { get; }

        public string Title { get; }

        public int ParameterCount { get; }

        public int OutOfRangeCount { get; }

        public string Subtitle => OutOfRangeCount == 0
            ? string.Format(S.Hist_SubtitleOk, ParameterCount)
            : string.Format(S.Hist_SubtitleOut, ParameterCount, OutOfRangeCount);

        public Color StatusColor => OutOfRangeCount == 0 ? StatusPalette.Normal : StatusPalette.High;
    }
}
