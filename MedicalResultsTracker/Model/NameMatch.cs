using System.Text;

namespace MedicalResultsTracker.Model
{
    /// <summary>
    /// Узнаёт показатель по названию, написанному иначе.
    ///
    /// Названия приходят из трёх мест — из бланка, из ответа чат-бота и из рук человека, —
    /// и совпадают они далеко не всегда: «Hämoglobin», «Hamoglobin», «Hb», «Hämoglobin (HGB)».
    /// Точное сравнение строк такие пары не ловит, и один показатель тихо расходится на два:
    /// две строки в таблице, два графика, и в каждом половина истории.
    ///
    /// Разница в регистре, умляутах и знаках снимается здесь молча — это заведомо одно и то же.
    /// Всё остальное — только предположение, и решает его человек: приложение не имеет права
    /// само склеить два показателя, потому что ошибка склейки испортит обе истории сразу.
    /// </summary>
    public static class NameMatch
    {
        /// <summary>Ниже этой близости пара уже не похожа, а просто состоит из тех же букв.</summary>
        private const double Threshold = 0.8;

        /// <summary>Сокращения («Hb») сравниваются иначе, чем слова: у них своё правило.</summary>
        private const int ShortName = 5;

        /// <summary>
        /// Название без того, что не меняет смысла: регистр, умляуты, пробелы, скобки, дефисы.
        /// «Vitamin B12», «vitamin-b12» и «VITAMIN B 12» дают одну и ту же строку.
        /// </summary>
        public static string Normalize(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            StringBuilder builder = new(name.Length);

            foreach (char symbol in name.Trim().ToLowerInvariant())
            {
                switch (symbol)
                {
                    case 'ä': builder.Append('a'); break;
                    case 'ö': builder.Append('o'); break;
                    case 'ü': builder.Append('u'); break;
                    case 'ß': builder.Append("ss"); break;
                    case 'ё': builder.Append('е'); break;
                    default:
                        if (char.IsLetterOrDigit(symbol))
                        {
                            builder.Append(symbol);
                        }

                        break;
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Одно и то же название, записанное по-разному. Спрашивать здесь не о чем —
        /// разница только в оформлении.
        /// </summary>
        public static bool AreSame(string? left, string? right)
        {
            string a = Normalize(left);

            return a.Length > 0 && a == Normalize(right);
        }

        /// <summary>
        /// Записи справочника, на которые название похоже, — от самой похожей к менее похожей.
        /// Точное совпадение сюда не попадает: его не предлагают, а используют.
        /// </summary>
        public static IReadOnlyList<Analyte> Candidates(
            string? name,
            IEnumerable<Analyte> catalog,
            int limit = 4)
        {
            string source = Normalize(name);

            if (source.Length == 0)
            {
                return Array.Empty<Analyte>();
            }

            return catalog
                .Where(a => !a.IsHidden && !string.IsNullOrWhiteSpace(a.Name))
                .Select(a => (Analyte: a, Score: Score(source, Normalize(a.Name))))
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Analyte.Name, StringComparer.CurrentCulture)
                .Take(limit)
                .Select(x => x.Analyte)
                .ToList();
        }

        /// <summary>
        /// Насколько две уже нормализованные строки похожи. 0 — не предлагать.
        /// </summary>
        private static double Score(string left, string right)
        {
            if (right.Length == 0 || left == right)
            {
                return 0;
            }

            // Одно название целиком внутри другого: «Ferritin» и «Ferritin (Serum)»,
            // «GPT (ALT)» и «ALT». Двухбуквенные обрывки так не сравниваем — под них
            // подходит слишком многое.
            if (left.Length >= 3 && right.Length >= 3 && (left.Contains(right) || right.Contains(left)))
            {
                return 0.95;
            }

            // Сокращение: все буквы короткого идут в длинном по порядку и начинаются одинаково.
            // Так «Hb» находит «Hämoglobin», но не «Albumin».
            string shorter = left.Length <= right.Length ? left : right;
            string longer = shorter == left ? right : left;

            if (shorter.Length is >= 2 and <= ShortName && shorter[0] == longer[0] && IsSubsequence(shorter, longer))
            {
                return 0.85;
            }

            // Опечатки и разные транслитерации: «Leukozyten» и «Leukocyten».
            // На коротких строках расстояние в одну букву — это уже другой показатель
            // («fT3» и «fT4»), поэтому их сюда не пускаем.
            if (shorter.Length < 4)
            {
                return 0;
            }

            double ratio = 1.0 - (double)Distance(left, right) / Math.Max(left.Length, right.Length);

            return ratio >= Threshold ? ratio : 0;
        }

        /// <summary>Буквы короткой строки встречаются в длинной по порядку.</summary>
        private static bool IsSubsequence(string shorter, string longer)
        {
            int index = 0;

            foreach (char symbol in longer)
            {
                if (index < shorter.Length && symbol == shorter[index])
                {
                    index++;
                }
            }

            return index == shorter.Length;
        }

        /// <summary>Расстояние Левенштейна: сколько правок отделяет одну строку от другой.</summary>
        private static int Distance(string left, string right)
        {
            int[] previous = new int[right.Length + 1];
            int[] current = new int[right.Length + 1];

            for (int j = 0; j <= right.Length; j++)
            {
                previous[j] = j;
            }

            for (int i = 1; i <= left.Length; i++)
            {
                current[0] = i;

                for (int j = 1; j <= right.Length; j++)
                {
                    int cost = left[i - 1] == right[j - 1] ? 0 : 1;

                    current[j] = Math.Min(
                        Math.Min(current[j - 1] + 1, previous[j] + 1),
                        previous[j - 1] + cost);
                }

                (previous, current) = (current, previous);
            }

            return previous[right.Length];
        }
    }
}
