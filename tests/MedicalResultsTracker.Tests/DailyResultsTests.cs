using MedicalResultsTracker.Model;
using Xunit;

namespace MedicalResultsTracker.Tests
{
    /// <summary>
    /// День как единица сравнения. За одно число бланков бывает несколько — вторая
    /// лаборатория, дозаписанная позже фотография, — и если брать из них один,
    /// на главном экране получится неверное число показателей вне нормы.
    /// </summary>
    public class DailyResultsTests
    {
        private static BloodTest Test(string date, DateTime modified, params BloodParameter[] parameters) => new()
        {
            Id = Guid.NewGuid(),
            Date = DateTime.Parse(date),
            ModifiedUtc = modified,
            Parameters = parameters.ToList(),
        };

        private static BloodParameter Value(string name, double value, double? max = null) => new()
        {
            Name = name,
            Value = value,
            RefMax = max,
        };

        [Fact]
        public void OneFormPerDayStaysAsItIs()
        {
            BloodTest single = Test("2026-03-12", new DateTime(2026, 3, 12), Value("Ferritin", 18));

            Assert.Same(single, DailyResults.Merge(new[] { single }));
        }

        /// <summary>Ровно тот случай, из-за которого счётчик на главном экране врал.</summary>
        [Fact]
        public void TwoFormsOfOneDayCountAsOne()
        {
            List<BloodTest> day = new()
            {
                Test("2026-03-12", new DateTime(2026, 3, 12, 9, 0, 0), Value("Ferritin", 18)),
                Test("2026-03-12", new DateTime(2026, 3, 12, 18, 0, 0), Value("Eisen", 45), Value("TSH", 2.1)),
            };

            BloodTest merged = DailyResults.Merge(day);

            Assert.Equal(3, merged.Parameters.Count);
        }

        [Fact]
        public void LaterFormWinsWhenTheSameValueIsRecordedTwice()
        {
            List<BloodTest> day = new()
            {
                Test("2026-03-12", new DateTime(2026, 3, 12, 9, 0, 0), Value("Ferritin", 18)),
                Test("2026-03-12", new DateTime(2026, 3, 12, 18, 0, 0), Value("Ferritin", 24)),
            };

            BloodParameter merged = DailyResults.Merge(day).Parameters.Single();

            Assert.Equal(24, merged.Value);
        }

        /// <summary>
        /// Значение из второго бланка того же дня обязано попасть в счёт «вне нормы»:
        /// иначе человек видит на главном экране меньше, чем есть на самом деле.
        /// </summary>
        [Fact]
        public void OutOfRangeValueFromTheSecondFormIsNotLost()
        {
            List<BloodTest> day = new()
            {
                Test("2026-03-12", new DateTime(2026, 3, 12, 9, 0, 0), Value("Ferritin", 18, max: 300)),
                Test("2026-03-12", new DateTime(2026, 3, 12, 18, 0, 0), Value("CRP", 12, max: 5)),
            };

            BloodTest merged = DailyResults.Merge(day);

            Assert.Equal(1, merged.Parameters.Count(p => p.Status == ParameterStatus.High));
        }

        [Fact]
        public void DaysComeNewestFirstAndOneEntryPerDay()
        {
            List<BloodTest> tests = new()
            {
                Test("2026-03-12", new DateTime(2026, 3, 12, 9, 0, 0), Value("Ferritin", 18)),
                Test("2026-03-12", new DateTime(2026, 3, 12, 18, 0, 0), Value("Eisen", 45)),
                Test("2026-01-05", new DateTime(2026, 1, 5), Value("Ferritin", 12)),
            };

            List<BloodTest> byDay = DailyResults.ByDay(tests);

            Assert.Equal(2, byDay.Count);
            Assert.Equal(new DateTime(2026, 3, 12), byDay[0].Date);
            Assert.Equal(new DateTime(2026, 1, 5), byDay[1].Date);
            Assert.Equal(2, byDay[0].Parameters.Count);
        }

        /// <summary>
        /// Время в дате не должно разбивать один день на два: дата сдачи хранится
        /// без времени, но данные приходят и из бэкапов, где оно могло сохраниться.
        /// </summary>
        [Fact]
        public void TimeOfDayDoesNotSplitTheDay()
        {
            List<BloodTest> tests = new()
            {
                Test("2026-03-12T08:30", new DateTime(2026, 3, 12, 9, 0, 0), Value("Ferritin", 18)),
                Test("2026-03-12T19:45", new DateTime(2026, 3, 12, 20, 0, 0), Value("Eisen", 45)),
            };

            Assert.Single(DailyResults.ByDay(tests));
        }
    }
}
