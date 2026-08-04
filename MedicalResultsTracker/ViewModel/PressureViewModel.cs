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
        /// <summary>Сколько последних измерений показывать на графике.</summary>
        private const int MaxChartPoints = 60;

        private readonly IBloodPressureRepository _repository;

        [ObservableProperty]
        private bool _isEmpty = true;

        [ObservableProperty]
        private string _targetSummary = string.Empty;

        [ObservableProperty]
        private PressureChartDrawable _chart = new();

        /// <summary>По одной точке линию не построишь — до второго измерения графика нет.</summary>
        [ObservableProperty]
        private bool _hasChart;

        [ObservableProperty]
        private string _chartLegend = string.Empty;

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
            int systolicLow = BloodPressureTarget.SystolicLow;
            int diastolicLow = BloodPressureTarget.DiastolicLow;

            // Нижние пороги можно выключить нулём — тогда и говорить о них незачем.
            TargetSummary = systolicLow > 0 || diastolicLow > 0
                ? string.Format(S.Bp_TargetSummaryLow, systolicTarget, diastolicTarget, systolicLow, diastolicLow)
                : string.Format(S.Bp_TargetSummary, systolicTarget, diastolicTarget);

            IReadOnlyList<BloodPressureReading> readings = await _repository.GetAllAsync();

            Readings.Clear();

            foreach (BloodPressureReading reading in readings)
            {
                Readings.Add(new PressureItemViewModel(
                    reading, systolicTarget, diastolicTarget, systolicLow, diastolicLow));
            }

            IsEmpty = Readings.Count == 0;

            BuildChart(readings, systolicTarget, diastolicTarget, systolicLow, diastolicLow);
        }

        /// <summary>
        /// Последние измерения от старых к новым. Ограничение по количеству — ради читаемости:
        /// на ширине телефона две сотни точек сливаются в сплошную полосу.
        /// </summary>
        private void BuildChart(
            IReadOnlyList<BloodPressureReading> readings,
            int systolic,
            int diastolic,
            int systolicLow,
            int diastolicLow)
        {
            List<BloodPressureReading> forChart = readings
                .Take(MaxChartPoints)
                .OrderBy(r => r.MeasuredAt)
                .ToList();

            Chart = new PressureChartDrawable
            {
                Readings = forChart,
                TargetSystolic = systolic,
                TargetDiastolic = diastolic,
                TargetSystolicLow = systolicLow,
                TargetDiastolicLow = diastolicLow,
            };

            HasChart = forChart.Count >= 2;
            ChartLegend = string.Format(S.Bp_ChartLegend, forChart.Count);
        }
    }

    /// <summary>Строка дневника.</summary>
    public sealed class PressureItemViewModel
    {
        public PressureItemViewModel(
            BloodPressureReading reading,
            int systolicTarget,
            int diastolicTarget,
            int systolicLow,
            int diastolicLow)
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
            IsBelowTarget = !IsAboveTarget && reading.IsBelow(systolicLow, diastolicLow);

            // Цветом отмечается выход за пороги, которые человек задал сам, — вверх красным,
            // вниз синим. Ни степеней, ни диагнозов: это сравнение с числом из настроек.
            Color = IsAboveTarget
                ? StatusPalette.High
                : IsBelowTarget ? StatusPalette.Low : StatusPalette.Normal;
        }

        public Guid Id { get; }

        public string Value { get; }

        public string When { get; }

        public string Pulse { get; }

        public bool HasPulse { get; }

        public string Note { get; }

        public bool HasNote { get; }

        public bool IsAboveTarget { get; }

        public bool IsBelowTarget { get; }

        public Color Color { get; }
    }
}
