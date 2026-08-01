using System.Globalization;
using MedicalResultsTracker.Controls;
using MedicalResultsTracker.Model;
using MedicalResultsTracker.Resources.Strings;
using MedicalResultsTracker.Services.Database;
using MedicalResultsTracker.Services.UI;

namespace MedicalResultsTracker.ViewModel
{
    /// <summary>Дневник давления: список измерений, свежие сверху.</summary>
    public partial class PressureViewModel : BaseViewModel
    {
        private readonly IBloodPressureRepository _repository;

        [ObservableProperty]
        private bool _isEmpty = true;

        [ObservableProperty]
        private string _targetSummary = string.Empty;

        public PressureViewModel(IBloodPressureRepository repository)
        {
            _repository = repository;

            Title = S.Bp_Title;
        }

        public ObservableCollection<PressureItemViewModel> Readings { get; } = new();

        public override Task InitializeAsync() => RunAsync(LoadAsync, S.Err_Bp);

        [RelayCommand]
        private Task Refresh() => RunAsync(LoadAsync, S.Err_Bp);

        [RelayCommand]
        private Task Add() => Shell.Current.GoToAsync(AppRoutes.PressureEdit);

        [RelayCommand]
        private Task Open(PressureItemViewModel? item) => item is null
            ? Task.CompletedTask
            : Shell.Current.GoToAsync($"{AppRoutes.PressureEdit}?{AppRoutes.PressureIdParameter}={item.Id}");

        private async Task LoadAsync()
        {
            Title = S.Bp_Title;

            int systolicTarget = BloodPressureTarget.Systolic;
            int diastolicTarget = BloodPressureTarget.Diastolic;

            TargetSummary = string.Format(S.Bp_TargetSummary, systolicTarget, diastolicTarget);

            IReadOnlyList<BloodPressureReading> readings = await _repository.GetAllAsync();

            Readings.Clear();

            foreach (BloodPressureReading reading in readings)
            {
                Readings.Add(new PressureItemViewModel(reading, systolicTarget, diastolicTarget));
            }

            IsEmpty = Readings.Count == 0;
        }
    }

    /// <summary>Строка дневника.</summary>
    public sealed class PressureItemViewModel
    {
        public PressureItemViewModel(BloodPressureReading reading, int systolicTarget, int diastolicTarget)
        {
            Id = reading.Id;
            Value = reading.Display;

            // Дата и время вместе: без времени два измерения за день не различить.
            When = reading.MeasuredAt.ToString("g", CultureInfo.CurrentCulture);

            Pulse = reading.Pulse is int pulse
                ? string.Format(S.Bp_PulseValue, pulse)
                : string.Empty;

            HasPulse = reading.Pulse is not null;
            Note = reading.Note ?? string.Empty;
            HasNote = !string.IsNullOrWhiteSpace(reading.Note);

            IsAboveTarget = reading.IsAbove(systolicTarget, diastolicTarget);

            // Цветом отмечается только «выше заданной цели» — без степеней и диагнозов.
            Color = IsAboveTarget ? StatusPalette.High : StatusPalette.Normal;
        }

        public Guid Id { get; }

        public string Value { get; }

        public string When { get; }

        public string Pulse { get; }

        public bool HasPulse { get; }

        public string Note { get; }

        public bool HasNote { get; }

        public bool IsAboveTarget { get; }

        public Color Color { get; }
    }
}
