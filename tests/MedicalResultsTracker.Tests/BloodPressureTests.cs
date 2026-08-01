using MedicalResultsTracker.Model;
using Xunit;

namespace MedicalResultsTracker.Tests
{
    /// <summary>
    /// Давление. Проверяется ровно то, что может испортить запись: опечатка в одну цифру
    /// превращает 120 в 1200, а перепутанные местами верхнее и нижнее — в бессмыслицу.
    /// </summary>
    public class BloodPressureTests
    {
        private static BloodPressureReading Reading(int systolic, int diastolic, int? pulse = null) => new()
        {
            Systolic = systolic,
            Diastolic = diastolic,
            Pulse = pulse,
        };

        [Theory]
        [InlineData(120, 80)]
        [InlineData(145, 95)]
        [InlineData(90, 60)]
        public void AcceptsOrdinaryReadings(int systolic, int diastolic) =>
            Assert.True(Reading(systolic, diastolic).IsPlausible);

        [Theory]
        // Лишняя цифра — самая частая опечатка на цифровой клавиатуре.
        [InlineData(1200, 80)]
        [InlineData(120, 800)]
        // Верхнее и нижнее перепутаны местами.
        [InlineData(80, 120)]
        // Равные значения давлением не бывают.
        [InlineData(120, 120)]
        // Пустая форма.
        [InlineData(0, 0)]
        public void RejectsWhatCannotBeABloodPressure(int systolic, int diastolic) =>
            Assert.False(Reading(systolic, diastolic).IsPlausible);

        [Theory]
        [InlineData(60, true)]
        [InlineData(200, true)]
        [InlineData(600, false)]
        [InlineData(5, false)]
        public void ChecksThePulseTooWhenItIsGiven(int pulse, bool expected) =>
            Assert.Equal(expected, Reading(120, 80, pulse).IsPlausible);

        [Fact]
        public void AllowsAMissingPulseBecauseNotEveryDeviceShowsIt() =>
            Assert.True(Reading(120, 80).IsPlausible);

        /// <summary>
        /// Подсветка идёт по порогу, который задал пользователь. Превышения по любому
        /// из двух значений достаточно: 135/95 — это уже выше цели 140/90.
        /// </summary>
        [Theory]
        [InlineData(120, 80, false)]
        [InlineData(145, 85, true)]
        [InlineData(135, 95, true)]
        [InlineData(140, 90, false)]
        public void MarksReadingsAboveTheTargetTheUserSet(int systolic, int diastolic, bool expected) =>
            Assert.Equal(expected, Reading(systolic, diastolic).IsAbove(140, 90));

        [Fact]
        public void ShowsTheReadingTheWayItIsSpoken() =>
            Assert.Equal("130/85", Reading(130, 85).Display);
    }
}
