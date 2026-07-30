using SQLite;

namespace MedicalResultsTracker.Model
{
    /// <summary>
    /// Каталог показателей (справочник). Хранит "как обычно называется" показатель,
    /// единицы измерения по умолчанию и типовой референсный диапазон.
    /// Диапазон из конкретного бланка лаборатории всегда важнее и хранится в <see cref="BloodParameter"/>.
    /// </summary>
    [Table("analytes")]
    public class Analyte
    {
        /// <summary>Стабильный код показателя, например "HGB" или "VIT_D".</summary>
        [PrimaryKey]
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? Unit { get; set; }

        /// <summary>
        /// Группа для отображения: "Общий анализ крови", "Липиды", ... Задаётся пользователем,
        /// встроенные значения — только стартовая раскладка, её можно перекроить под себя.
        /// </summary>
        public string? Category { get; set; }

        /// <summary>Показатель, за которым пользователь следит: выносится наверх на главный экран.</summary>
        public bool IsFavorite { get; set; }

        public double? RefMin { get; set; }

        public double? RefMax { get; set; }

        /// <summary>Пояснение к диапазону (зависимость от пола, возраста и т.п.).</summary>
        public string? Notes { get; set; }

        /// <summary>true — показатель из встроенного справочника, false — добавлен пользователем.</summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// Скрыт из подсказок при вводе. Встроенные показатели не удаляются, а прячутся:
        /// удалённый вернулся бы при следующем запуске, а список подсказок должен быть коротким.
        /// </summary>
        public bool IsHidden { get; set; }

        public int SortOrder { get; set; }

        [Ignore]
        public ReferenceRange DefaultRange => new() { Min = RefMin, Max = RefMax, Notes = Notes };
    }
}
