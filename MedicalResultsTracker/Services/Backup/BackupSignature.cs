using System.Globalization;

namespace MedicalResultsTracker.Services.Backup
{
    /// <summary>
    /// Отпечаток состояния данных. Совпал с сохранённым — значит с прошлой копии ничего
    /// не менялось и писать нечего.
    ///
    /// Одного «когда меняли в последний раз» мало: удали самую свежую запись — и время
    /// последнего изменения станет **меньше**, то есть удаление осталось бы незамеченным.
    /// Поэтому вместе с ним считаются и количества.
    /// </summary>
    public readonly record struct BackupSignature(int Tests, int Pressure, long LastChangeTicks)
    {
        public override string ToString() =>
            string.Create(CultureInfo.InvariantCulture, $"{Tests}.{Pressure}.{LastChangeTicks}");

        public static BackupSignature? Parse(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            string[] parts = text.Split('.');

            if (parts.Length != 3 ||
                !int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int tests) ||
                !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int pressure) ||
                !long.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out long ticks))
            {
                return null;
            }

            return new BackupSignature(tests, pressure, ticks);
        }
    }
}
