namespace MedicalResultsTracker.Model
{
    /// <summary>
    /// День как единица сравнения.
    ///
    /// За одно число бланков может быть несколько: две лаборатории, дозаписанная позже
    /// фотография, правка старой записи с изменением даты. В таблице такой день давно
    /// показывается одним столбцом, а вот «последний анализ» брался одной строкой из базы —
    /// и всё, что было в остальных бланках того же дня, в счёт не шло. На главном экране
    /// это выглядело как неверное число показателей вне нормы, а «заметные изменения»
    /// сравнивали два бланка одного и того же дня между собой.
    /// </summary>
    public static class DailyResults
    {
        /// <summary>
        /// Сводит бланки одного дня в одну запись. Если один и тот же показатель есть
        /// в нескольких бланках, побеждает записанный позже — то же правило, по которому
        /// собирается столбец таблицы.
        /// </summary>
        public static BloodTest Merge(IReadOnlyList<BloodTest> sameDay)
        {
            if (sameDay.Count == 1)
            {
                return sameDay[0];
            }

            Dictionary<string, BloodParameter> byKey = new(StringComparer.Ordinal);

            foreach (BloodTest test in sameDay.OrderBy(t => t.ModifiedUtc))
            {
                foreach (BloodParameter parameter in test.Parameters)
                {
                    byKey[AnalyteCode.KeyOf(parameter.Code, parameter.Name)] = parameter;
                }
            }

            BloodTest newest = sameDay.OrderByDescending(t => t.ModifiedUtc).First();

            return new BloodTest
            {
                Id = newest.Id,
                Date = newest.Date,
                Laboratory = newest.Laboratory,
                Notes = newest.Notes,
                Origin = newest.Origin,
                SourceFilePath = newest.SourceFilePath,
                CreatedUtc = newest.CreatedUtc,
                ModifiedUtc = newest.ModifiedUtc,
                Parameters = byKey.Values.OrderBy(p => p.SortOrder).ThenBy(p => p.Name).ToList(),
            };
        }

        /// <summary>
        /// Раскладывает бланки по дням, свежий день первым. Внутри дня — уже одна сведённая запись.
        /// </summary>
        public static List<BloodTest> ByDay(IReadOnlyList<BloodTest> tests) => tests
            .GroupBy(test => test.Date.Date)
            .OrderByDescending(group => group.Key)
            .Select(group => Merge(group.ToList()))
            .ToList();
    }
}
