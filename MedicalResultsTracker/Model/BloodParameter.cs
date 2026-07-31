using System.Text.Json.Serialization;
using SQLite;

namespace MedicalResultsTracker.Model
{
    /// <summary>Одна строка таблицы результатов: показатель, значение, единицы и норма этой лаборатории.</summary>
    [Table("blood_parameters")]
    public class BloodParameter
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Indexed]
        public Guid TestId { get; set; }

        /// <summary>Код показателя из каталога. Пусто, если показатель разовый и в каталог не занесён.</summary>
        [Indexed]
        public string? Code { get; set; }

        /// <summary>Название так, как оно напечатано в бланке.</summary>
        public string Name { get; set; } = string.Empty;

        public string? Unit { get; set; }

        /// <summary>Числовое значение. null, если результат качественный — тогда заполнен <see cref="TextValue"/>.</summary>
        public double? Value { get; set; }

        /// <summary>Нечисловой результат: "отрицательно", "следы", "&lt; 0.5".</summary>
        public string? TextValue { get; set; }

        public double? RefMin { get; set; }

        public double? RefMax { get; set; }

        public string? Comment { get; set; }

        public int SortOrder { get; set; }

        [Ignore]
        [JsonIgnore]
        public ReferenceRange Range => new() { Min = RefMin, Max = RefMax };

        [Ignore]
        [JsonIgnore]
        public ParameterStatus Status
        {
            get
            {
                if (Value is not double value)
                {
                    return ParameterStatus.Unknown;
                }

                if (RefMin is double min && value < min)
                {
                    return ParameterStatus.Low;
                }

                if (RefMax is double max && value > max)
                {
                    return ParameterStatus.High;
                }

                return RefMin is null && RefMax is null
                    ? ParameterStatus.Unknown
                    : ParameterStatus.Normal;
            }
        }

        /// <summary>Значение для показа: число или текстовый результат.</summary>
        [Ignore]
        [JsonIgnore]
        public string DisplayValue => Value?.ToString("0.####") ?? TextValue ?? "—";

        /// <summary>
        /// Насколько значение выходит за норму. 0 — внутри диапазона.
        /// Используется, чтобы определить, стало лучше или хуже по сравнению с прошлым разом.
        /// </summary>
        [Ignore]
        [JsonIgnore]
        public double? DistanceFromRange
        {
            get
            {
                if (Value is not double value)
                {
                    return null;
                }

                if (RefMin is double min && value < min)
                {
                    return min - value;
                }

                if (RefMax is double max && value > max)
                {
                    return value - max;
                }

                return RefMin is null && RefMax is null ? null : 0d;
            }
        }

        public BloodParameter Clone() => new()
        {
            Id = Guid.NewGuid(),
            TestId = TestId,
            Code = Code,
            Name = Name,
            Unit = Unit,
            Value = Value,
            TextValue = TextValue,
            RefMin = RefMin,
            RefMax = RefMax,
            Comment = Comment,
            SortOrder = SortOrder,
        };
    }
}
