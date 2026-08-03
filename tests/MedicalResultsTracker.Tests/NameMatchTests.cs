using MedicalResultsTracker.Model;
using Xunit;

namespace MedicalResultsTracker.Tests
{
    /// <summary>
    /// Узнавание показателя по названию. Ошибка в любую сторону дорогая: не узнали —
    /// история расходится на два графика по половине значений; узнали лишнее —
    /// человеку предлагают склеить разные показатели.
    /// </summary>
    public class NameMatchTests
    {
        private static readonly Analyte[] Catalog =
        {
            New("HGB", "Hämoglobin", "g/dl"),
            New("FERR", "Ferritin", "ng/ml"),
            New("ALT", "GPT (ALT)", "U/l"),
            New("FT3", "fT3", "pg/ml"),
            New("FT4", "fT4", "ng/dl"),
            New("K", "Kalium", "mmol/l"),
            New("CA", "Calcium", "mmol/l"),
            New("B12", "Vitamin B12", "pg/ml"),
        };

        private static Analyte New(string code, string name, string unit) =>
            new() { Code = code, Name = name, Unit = unit };

        private static string[] Names(string typed) =>
            NameMatch.Candidates(typed, Catalog).Select(a => a.Name).ToArray();

        [Theory]
        // Регистр, умляуты и знаки смысла не меняют.
        [InlineData("Hämoglobin", "Hamoglobin")]
        [InlineData("Vitamin B12", "vitamin-b12")]
        [InlineData("Vitamin B12", "VITAMIN  B12")]
        [InlineData("GPT (ALT)", "gpt alt")]
        public void SameNameWrittenDifferentlyIsTheSameName(string left, string right) =>
            Assert.True(NameMatch.AreSame(left, right));

        [Theory]
        [InlineData("Kalium", "Calcium")]
        [InlineData("fT3", "fT4")]
        [InlineData("Ferritin", "Eisen")]
        public void DifferentValuesAreNotTheSameName(string left, string right) =>
            Assert.False(NameMatch.AreSame(left, right));

        /// <summary>Сокращение из бланка должно находить полное название.</summary>
        [Fact]
        public void AbbreviationFindsTheFullName() => Assert.Contains("Hämoglobin", Names("Hb"));

        /// <summary>Уточнение в скобках — тот же показатель, а не второй.</summary>
        [Fact]
        public void ExtraQualifierStillPointsAtTheSameValue() =>
            Assert.Contains("Ferritin", Names("Ferritin (Serum)"));

        /// <summary>Другая часть составного названия тоже должна узнаваться.</summary>
        [Fact]
        public void PartOfACompoundNameIsFound() => Assert.Contains("GPT (ALT)", Names("ALT"));

        /// <summary>Разная транслитерация — одна и та же строка бланка.</summary>
        [Fact]
        public void TransliterationDifferenceIsRecognised() => Assert.Contains("Ferritin", Names("Feritin"));

        /// <summary>
        /// Самое опасное: fT3 и fT4 отличаются одной цифрой, но это разные показатели.
        /// Предлагать их друг вместо друга нельзя — человек согласится не глядя.
        /// </summary>
        [Fact]
        public void ValuesThatDifferByOneCharacterAreNeverOffered()
        {
            Assert.DoesNotContain("fT4", Names("fT3"));
            Assert.DoesNotContain("fT3", Names("fT4"));
        }

        [Fact]
        public void SimilarlySpelledDifferentValuesAreNotOffered() =>
            Assert.DoesNotContain("Calcium", Names("Kalium"));

        /// <summary>Точное совпадение — не предположение: его используют, а не предлагают.</summary>
        [Fact]
        public void ExactMatchIsNotOfferedAsAGuess() => Assert.DoesNotContain("Ferritin", Names("Ferritin"));

        [Fact]
        public void UnknownNameOffersNothing() => Assert.Empty(Names("Borrelien IgG"));

        [Fact]
        public void HiddenValuesAreNotOffered()
        {
            Analyte[] catalog = { new() { Code = "FERR", Name = "Ferritin", IsHidden = true } };

            Assert.Empty(NameMatch.Candidates("Feritin", catalog));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("—")]
        public void EmptyNameOffersNothingInsteadOfThrowing(string? typed) =>
            Assert.Empty(NameMatch.Candidates(typed, Catalog));
    }
}
