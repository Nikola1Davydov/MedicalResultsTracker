using System.Globalization;
using MedicalResultsTracker.Controls;
using MedicalResultsTracker.Model;

namespace MedicalResultsTracker.ViewModel
{
    /// <summary>Редактируемая строка таблицы результатов.</summary>
    public partial class ParameterRowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string? _unit;

        [ObservableProperty]
        private string _valueText = string.Empty;

        [ObservableProperty]
        private string _refMinText = string.Empty;

        [ObservableProperty]
        private string _refMaxText = string.Empty;

        [ObservableProperty]
        private string? _comment;

        [ObservableProperty]
        private string _statusText = string.Empty;

        [ObservableProperty]
        private Color _statusColor = StatusPalette.Unknown;

        public ParameterRowViewModel()
        {
            Id = Guid.NewGuid();
        }

        public ParameterRowViewModel(BloodParameter parameter)
        {
            Id = parameter.Id;
            Code = parameter.Code;
            _name = parameter.Name;
            _unit = parameter.Unit;
            _valueText = parameter.Value?.ToString("0.####", CultureInfo.CurrentCulture) ?? parameter.TextValue ?? string.Empty;
            _refMinText = Format(parameter.RefMin);
            _refMaxText = Format(parameter.RefMax);
            _comment = parameter.Comment;

            RefreshStatus();
        }

        public Guid Id { get; }

        /// <summary>Код показателя из каталога, если строка добавлена оттуда.</summary>
        public string? Code { get; set; }

        public bool IsEmpty => string.IsNullOrWhiteSpace(Name) && string.IsNullOrWhiteSpace(ValueText);

        public static ParameterRowViewModel FromAnalyte(Analyte analyte) => new()
        {
            Code = analyte.Code,
            Name = analyte.Name,
            Unit = analyte.Unit,
            RefMinText = Format(analyte.RefMin),
            RefMaxText = Format(analyte.RefMax),
        };

        public BloodParameter ToModel(Guid testId, int sortOrder)
        {
            double? value = ParseNumber(ValueText);

            return new BloodParameter
            {
                Id = Id,
                TestId = testId,
                Code = Code,
                Name = Name.Trim(),
                Unit = string.IsNullOrWhiteSpace(Unit) ? null : Unit.Trim(),
                Value = value,
                TextValue = value is null && !string.IsNullOrWhiteSpace(ValueText) ? ValueText.Trim() : null,
                RefMin = ParseNumber(RefMinText),
                RefMax = ParseNumber(RefMaxText),
                Comment = string.IsNullOrWhiteSpace(Comment) ? null : Comment.Trim(),
                SortOrder = sortOrder,
            };
        }

        /// <summary>
        /// Принимает и «5.2», и «5,2»: на бланках и клавиатурах разделитель разный,
        /// заставлять пользователя думать об этом не нужно. Разбор общий с импортом из
        /// чата: раньше здесь была своя, более наивная версия, и одно и то же число
        /// в форме и во вставке читалось по-разному.
        /// </summary>
        public static double? ParseNumber(string? text) => LabNumber.Parse(text);

        /// <summary>Запись, которую нельзя прочитать однозначно, — о ней предупреждают перед сохранением.</summary>
        public bool HasAmbiguousValue
        {
            get
            {
                LabNumber.Parse(ValueText, out bool ambiguous);

                return ambiguous;
            }
        }

        partial void OnValueTextChanged(string value) => RefreshStatus();

        partial void OnRefMinTextChanged(string value) => RefreshStatus();

        partial void OnRefMaxTextChanged(string value) => RefreshStatus();

        private void RefreshStatus()
        {
            BloodParameter probe = new()
            {
                Name = Name,
                Value = ParseNumber(ValueText),
                RefMin = ParseNumber(RefMinText),
                RefMax = ParseNumber(RefMaxText),
            };

            ParameterStatus status = probe.Status;

            StatusColor = StatusPalette.For(status);
            StatusText = status == ParameterStatus.Unknown ? string.Empty : StatusPalette.Describe(status);
        }

        private static string Format(double? value) =>
            value?.ToString("0.####", CultureInfo.CurrentCulture) ?? string.Empty;
    }
}
