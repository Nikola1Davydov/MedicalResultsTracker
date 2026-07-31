namespace MedicalResultsTracker.Model
{
    /// <summary>
    /// Таблица «строка — показатель, столбец — дата».
    ///
    /// Один столбец — одна календарная дата, а не один бланк. Два бланка от одного числа
    /// (например, из разных лабораторий) — это всё равно один день, и разносить их
    /// по соседним столбцам с одинаковой шапкой незачем.
    /// </summary>
    public sealed class ResultMatrix
    {
        public List<DateTime> Dates { get; init; } = new();

        public List<MatrixLine> Lines { get; init; } = new();

        /// <summary>
        /// Последние <paramref name="count"/> столбцов. Строки, от которых в них ничего
        /// не осталось, выпадают: пустая строка в таблице только мешает.
        /// </summary>
        public ResultMatrix TakeLastDates(int count)
        {
            if (count <= 0 || count >= Dates.Count)
            {
                return this;
            }

            int skip = Dates.Count - count;

            return new ResultMatrix
            {
                Dates = Dates.Skip(skip).ToList(),
                Lines = Lines
                    .Select(line => new MatrixLine
                    {
                        Key = line.Key,
                        Newest = line.Newest,
                        Cells = line.Cells.Skip(skip).ToList(),
                    })
                    .Where(line => line.Cells.Any(cell => cell is not null))
                    .ToList(),
            };
        }
    }

    /// <summary>Одна строка таблицы: показатель и его значения по всем датам.</summary>
    public sealed class MatrixLine
    {
        public required string Key { get; init; }

        /// <summary>Название, единицы и норма берутся из самого свежего измерения.</summary>
        public required BloodParameter Newest { get; init; }

        /// <summary>По элементу на дату; null — в тот день показатель не сдавали.</summary>
        public required IReadOnlyList<BloodParameter?> Cells { get; init; }
    }
}
