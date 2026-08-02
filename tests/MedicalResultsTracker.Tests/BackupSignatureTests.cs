using MedicalResultsTracker.Services.Backup;
using Xunit;

namespace MedicalResultsTracker.Tests
{
    /// <summary>
    /// Отпечаток состояния данных. От него зависит, появится ли автоматическая копия:
    /// ошибётся он в одну сторону — копии перестанут делаться и пропажу данных будет
    /// нечем закрыть, ошибётся в другую — файлы полезут при каждом запуске.
    /// </summary>
    public class BackupSignatureTests
    {
        [Fact]
        public void SameDataGivesTheSameSignature() =>
            Assert.Equal(new BackupSignature(4, 12, 1000), new BackupSignature(4, 12, 1000));

        [Theory]
        // Добавили или удалили анализ.
        [InlineData(5, 12, 1000)]
        // Добавили или удалили измерение давления.
        [InlineData(4, 13, 1000)]
        // Ничего не добавляли, но что-то поправили.
        [InlineData(4, 12, 2000)]
        public void AnyChangeGivesADifferentSignature(int tests, int pressure, long ticks) =>
            Assert.NotEqual(new BackupSignature(4, 12, 1000), new BackupSignature(tests, pressure, ticks));

        /// <summary>
        /// Удаление самой свежей записи двигает «последнее изменение» **назад**. Одного времени
        /// было бы мало — именно поэтому в отпечатке есть ещё и количества.
        /// </summary>
        [Fact]
        public void NoticesDeletionEvenThoughTheTimestampGoesBackwards() =>
            Assert.NotEqual(new BackupSignature(4, 12, 5000), new BackupSignature(3, 12, 4000));

        [Fact]
        public void SurvivesTextRoundTrip()
        {
            BackupSignature original = new(4, 12, 638000000000000000);

            Assert.Equal(original, BackupSignature.Parse(original.ToString()));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("мусор")]
        [InlineData("1.2")]
        [InlineData("1.2.3.4")]
        [InlineData("a.b.c")]
        public void TreatsUnreadableStorageAsNoSignature(string? stored) =>
            Assert.Null(BackupSignature.Parse(stored));

        /// <summary>
        /// Пустого отпечатка достаточно, чтобы копия сделалась: сравнение с null не совпадёт
        /// ни с чем. Так первая копия появляется сразу после выбора папки.
        /// </summary>
        [Fact]
        public void MissingSignatureNeverMatchesRealData() =>
            Assert.NotEqual(BackupSignature.Parse("мусор"), new BackupSignature(0, 0, 0));
    }
}
