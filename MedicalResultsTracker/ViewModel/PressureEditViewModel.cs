using System.Globalization;
using MedicalResultsTracker.Model;
using MedicalResultsTracker.Resources.Strings;
using MedicalResultsTracker.Services.Database;
using MedicalResultsTracker.Services.UI;

namespace MedicalResultsTracker.ViewModel
{
    /// <summary>Ввод одного измерения давления. Дата и время подставляются текущие, но правятся.</summary>
    public partial class PressureEditViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IBloodPressureRepository _repository;

        [ObservableProperty]
        private DateTime _date = DateTime.Today;

        /// <summary>
        /// Время измерения. Утреннее и вечернее давление различаются, и без времени
        /// две записи за один день не отличить друг от друга.
        /// </summary>
        [ObservableProperty]
        private TimeSpan _time = DateTime.Now.TimeOfDay;

        [ObservableProperty]
        private string _systolic = string.Empty;

        [ObservableProperty]
        private string _diastolic = string.Empty;

        [ObservableProperty]
        private string _pulse = string.Empty;

        [ObservableProperty]
        private string? _note;

        [ObservableProperty]
        private bool _isExisting;

        private Guid _id = Guid.NewGuid();
        private DateTime _createdUtc = DateTime.UtcNow;

        public PressureEditViewModel(IBloodPressureRepository repository)
        {
            _repository = repository;

            Title = S.Bp_TitleNew;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue(AppRoutes.PressureIdParameter, out object? value) &&
                Guid.TryParse(Convert.ToString(value), out Guid id))
            {
                _id = id;
                IsExisting = true;
                Title = S.Bp_TitleExisting;
            }
            else
            {
                ResetToNew();
            }
        }

        public override Task InitializeAsync() => RunAsync(LoadAsync, S.Err_Bp);

        [RelayCommand]
        private Task Save() => RunAsync(async () =>
        {
            if (!TryReadNumber(Systolic, out int systolic) || !TryReadNumber(Diastolic, out int diastolic))
            {
                await Dialog.AlertAsync(S.Bp_BadValuesTitle, S.Bp_BadValuesBody);
                return;
            }

            int? pulse = null;

            if (!string.IsNullOrWhiteSpace(Pulse))
            {
                if (!TryReadNumber(Pulse, out int parsed))
                {
                    await Dialog.AlertAsync(S.Bp_BadValuesTitle, S.Bp_BadValuesBody);
                    return;
                }

                pulse = parsed;
            }

            BloodPressureReading reading = new()
            {
                Id = _id,
                MeasuredAt = Date.Date + Time,
                Systolic = systolic,
                Diastolic = diastolic,
                Pulse = pulse,
                Note = string.IsNullOrWhiteSpace(Note) ? null : Note.Trim(),
                CreatedUtc = _createdUtc,
            };

            // Опечатка в одну цифру превращает 120 в 1200. Здесь это ловится сразу,
            // а не всплывает потом на графике.
            if (!reading.IsPlausible)
            {
                await Dialog.AlertAsync(S.Bp_ImplausibleTitle, S.Bp_ImplausibleBody);
                return;
            }

            await _repository.SaveAsync(reading);

            await Shell.Current.GoToAsync("..");
        }, S.Err_BpSave);

        [RelayCommand]
        private Task Delete() => RunAsync(async () =>
        {
            if (!IsExisting)
            {
                await Shell.Current.GoToAsync("..");
                return;
            }

            if (!await Dialog.ConfirmAsync(S.Bp_DeleteTitle, S.Bp_DeleteBody, S.Common_Delete))
            {
                return;
            }

            await _repository.DeleteAsync(_id);
            await Shell.Current.GoToAsync("..");
        }, S.Err_BpDelete);

        [RelayCommand]
        private Task Cancel() => Shell.Current.GoToAsync("..");

        private async Task LoadAsync()
        {
            if (!IsExisting)
            {
                return;
            }

            if (await _repository.GetAsync(_id) is not BloodPressureReading reading)
            {
                ResetToNew();
                return;
            }

            Date = reading.MeasuredAt.Date;
            Time = reading.MeasuredAt.TimeOfDay;
            Systolic = reading.Systolic.ToString(CultureInfo.CurrentCulture);
            Diastolic = reading.Diastolic.ToString(CultureInfo.CurrentCulture);
            Pulse = reading.Pulse?.ToString(CultureInfo.CurrentCulture) ?? string.Empty;
            Note = reading.Note;
            _createdUtc = reading.CreatedUtc;
        }

        /// <summary>Цифровая клавиатура на Android пропускает и пробелы, и знаки — читаем терпимо.</summary>
        private static bool TryReadNumber(string text, out int value) =>
            int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.CurrentCulture, out value) ||
            int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

        private void ResetToNew()
        {
            _id = Guid.NewGuid();
            _createdUtc = DateTime.UtcNow;
            IsExisting = false;
            Title = S.Bp_TitleNew;

            // Сегодняшнее число и текущее время: обычно записывают сразу после измерения.
            Date = DateTime.Today;
            Time = DateTime.Now.TimeOfDay;

            Systolic = string.Empty;
            Diastolic = string.Empty;
            Pulse = string.Empty;
            Note = null;
        }
    }
}
