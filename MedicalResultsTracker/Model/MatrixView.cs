using SQLite;

namespace MedicalResultsTracker.Model
{
    /// <summary>
    /// Свой набор показателей для таблицы: «железо и кровь», «сердце», «щитовидка».
    ///
    /// Это не категория из справочника: у показателя одна категория, но входить он может
    /// в сколько угодно наборов. Поэтому связь отдельной таблицей, а не полем.
    /// </summary>
    [Table("matrix_views")]
    public class MatrixView
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>Коды показателей в том порядке, в каком их выбрал пользователь.</summary>
        [Ignore]
        public List<string> Codes { get; set; } = new();
    }

    /// <summary>Показатель внутри набора. Порядок сохраняется: пользователь выбирал его не случайно.</summary>
    [Table("matrix_view_items")]
    public class MatrixViewItem
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Indexed]
        public Guid ViewId { get; set; }

        public string Code { get; set; } = string.Empty;

        public int SortOrder { get; set; }
    }
}
