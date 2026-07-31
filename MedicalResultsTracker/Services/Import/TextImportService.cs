using System.Globalization;
using MedicalResultsTracker.Model;
using MedicalResultsTracker.Resources.Strings;
using MedicalResultsTracker.Services.Ai;

namespace MedicalResultsTracker.Services.Import
{
    /// <inheritdoc cref="ITextImportService"/>
    public sealed class TextImportService : ITextImportService
    {
        private static readonly char[] Separators = { '|', '\t', ';' };

        private static readonly string[] DateFormats =
        {
            "dd.MM.yyyy", "d.M.yyyy", "dd.MM.yy", "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy"
        };

        // Слова, по которым видно, что строка — заголовок таблицы, а не данные.
        private static readonly string[] HeaderWords =
        {
            "bezeichnung", "parameter", "wert", "ergebnis", "einheit", "referenz", "normbereich",
            "показатель", "название", "значение", "результат", "единиц", "норма", "референс"
        };

        /// <summary>
        /// Длинный список в запросе только мешает: чат-бот и так получает основную часть каталога,
        /// а всё, что человек реально сдаёт, помещается в эти рамки с запасом.
        /// </summary>
        private const int MaxKnownNames = 200;

        public string BuildPrompt(IReadOnlyList<Analyte> known)
        {
            string prompt =
                """
                Unten ist das Foto eines Laborbefunds. Übertrage die Daten daraus als reinen Text
                genau in diesem Format und ergänze nichts von dir aus.

                Die ersten beiden Zeilen:
                Datum: TT.MM.JJJJ
                Labor: Name oder Strich

                Danach eine Zeile je Wert, die Felder mit einem senkrechten Strich getrennt:
                Bezeichnung | Ergebnis | Einheit | Referenz von | Referenz bis

                Regeln:
                - Das Ergebnis ist nur eine Zahl. Ist es nicht numerisch, schreibe es als Wort
                  («negativ», «Spuren»).
                - Der Referenzbereich sind zwei getrennte Zahlen. Ist er einseitig («bis 5,2»),
                  fülle nur das passende Feld, das andere bleibt leer.
                - Rate nichts und rechne nichts um: Ist ein Feld im Befund nicht lesbar, lass es leer.
                - Keine Tabellenüberschrift, keine Nummerierung, keine Erläuterungen, keine
                  Schlussfolgerungen und keine Empfehlungen. Nur die Datenzeilen.
                """;

            string names = FormatKnownNames(known);

            if (names.Length == 0)
            {
                return prompt + Environment.NewLine + """
                    - Übernimm die Bezeichnungen genau wie im Befund: nicht übersetzen, nicht abkürzen,
                      nicht umbenennen.
                    """;
            }

            // Список идёт после правил: сначала формат, потом справочные данные к нему.
            return prompt + Environment.NewLine + """
                - Gleicht ein Wert einer Bezeichnung aus der Liste unten – auch abgekürzt («Hb»),
                  in einer anderen Sprache («Hemoglobin») oder anders geschrieben –, dann schreibe
                  die Bezeichnung genau so, wie sie in der Liste steht. Nur so landet der Wert in
                  derselben Zeile meiner Tabelle wie bisher und nicht in einer zweiten daneben.
                - Steht ein Wert nicht in der Liste, übernimm seine Bezeichnung unverändert
                  aus dem Befund: nicht übersetzen, nicht abkürzen, nicht umbenennen.
                - Zahl und Einheit bleiben immer so, wie sie im Befund gedruckt sind. Rechne
                  auch dann nicht um, wenn die Liste eine andere Einheit nennt – schreibe die
                  Einheit des Befunds hin. Die Angleichung betrifft ausschließlich die Bezeichnung.

                Bezeichnungen, die ich schon führe (Bezeichnung | Einheit):

                """ + names;
        }

        /// <summary>
        /// Названия с единицами, по строке на показатель. Избранное впереди: если список
        /// упрётся в предел, отрезать должно то, за чем человек не следит.
        /// </summary>
        private static string FormatKnownNames(IReadOnlyList<Analyte> known)
        {
            IEnumerable<string> lines = known
                .Where(a => !a.IsHidden && !string.IsNullOrWhiteSpace(a.Name))
                .OrderByDescending(a => a.IsFavorite)
                .ThenBy(a => a.Name, StringComparer.CurrentCulture)
                .Take(MaxKnownNames)
                .Select(a => string.IsNullOrWhiteSpace(a.Unit)
                    ? a.Name.Trim()
                    : $"{a.Name.Trim()} | {a.Unit!.Trim()}");

            return string.Join(Environment.NewLine, lines);
        }

        public AiDraft Parse(string text)
        {
            AiDraft draft = new();

            if (string.IsNullOrWhiteSpace(text))
            {
                draft.Warnings.Add(S.Imp_EmptyText);
                return draft;
            }

            bool headerSkipped = false;

            foreach (string rawLine in text.Split('\n'))
            {
                string line = rawLine.Trim().TrimEnd('\r');

                if (line.Length == 0 || IsMarkdownRule(line))
                {
                    continue;
                }

                if (TryReadHeaderField(line, out string dateValue, "datum", "дата"))
                {
                    draft.Date = ParseDate(dateValue);

                    if (draft.Date is null)
                    {
                        draft.Warnings.Add(string.Format(S.Imp_BadDate, dateValue));
                    }

                    continue;
                }

                if (TryReadHeaderField(line, out string lab, "labor", "лаборатория"))
                {
                    draft.Laboratory = lab is "-" or "—" or "" ? null : lab;
                    continue;
                }

                string[] parts = SplitRow(line);

                if (parts.Length < 2)
                {
                    draft.Warnings.Add(string.Format(S.Imp_SkippedLine, Shorten(line)));
                    continue;
                }

                // Заголовок таблицы пропускаем один раз: дальше такие строки — уже данные.
                if (!headerSkipped && LooksLikeHeader(parts))
                {
                    headerSkipped = true;
                    continue;
                }

                AiDraftRow? row = ParseRow(parts, out bool ambiguous);

                if (row is null)
                {
                    draft.Warnings.Add(string.Format(S.Imp_SkippedLine, Shorten(line)));
                    continue;
                }

                // Точка могла означать и разряды, и дробную часть. Прочитано как разряды,
                // но человек должен увидеть это до сохранения.
                if (ambiguous)
                {
                    draft.Warnings.Add(string.Format(S.Imp_AmbiguousNumber, row.Name, Get(parts, 1)));
                }

                draft.Rows.Add(row);
            }

            if (draft.Rows.Count == 0)
            {
                draft.Warnings.Add(S.Imp_NoRows);
            }

            return draft;
        }

        private static AiDraftRow? ParseRow(string[] parts, out bool ambiguous)
        {
            ambiguous = false;

            string name = parts[0].Trim();

            if (name.Length == 0)
            {
                return null;
            }

            string valueText = Get(parts, 1);
            double? value = ParseNumber(valueText, out ambiguous);

            AiDraftRow row = new()
            {
                Name = name,
                Unit = Empty(Get(parts, 2)),
                Value = value,
                TextValue = value is null ? Empty(valueText) : null,
            };

            // Норма может прийти двумя числами (как просили) или одной колонкой вида «30–300».
            if (parts.Length > 4)
            {
                row.RefMin = ParseNumber(Get(parts, 3));
                row.RefMax = ParseNumber(Get(parts, 4));
            }
            else if (parts.Length > 3)
            {
                (row.RefMin, row.RefMax) = ParseRange(Get(parts, 3));
            }

            return row;
        }

        /// <summary>Разбирает норму, записанную одной колонкой: «30–300», «до 5,2», «≥ 1».</summary>
        private static (double? Min, double? Max) ParseRange(string text)
        {
            string value = text.Trim();

            if (value.Length == 0)
            {
                return (null, null);
            }

            if (value.StartsWith('<') || value.StartsWith('≤') ||
                StartsWithWord(value, "bis", "до"))
            {
                return (null, ParseNumber(StripPrefix(value, '<', '≤')));
            }

            if (value.StartsWith('>') || value.StartsWith('≥') ||
                StartsWithWord(value, "ab", "von", "от"))
            {
                return (ParseNumber(StripPrefix(value, '>', '≥')), null);
            }

            string[] bounds = value.Split('–', '—', '-', '…');

            if (bounds.Length == 2)
            {
                return (ParseNumber(bounds[0]), ParseNumber(bounds[1]));
            }

            return (null, null);
        }

        /// <summary>Шапка вида «Datum: …» или «Дата: …»: язык ответа зависит от языка бланка.</summary>
        private static bool TryReadHeaderField(string line, out string value, params string[] fields)
        {
            value = string.Empty;

            int colon = line.IndexOf(':');

            if (colon <= 0)
            {
                return false;
            }

            string key = line[..colon].Trim();

            if (!fields.Any(f => key.StartsWith(f, StringComparison.CurrentCultureIgnoreCase)))
            {
                return false;
            }

            value = line[(colon + 1)..].Trim();

            return true;
        }

        private static DateTime? ParseDate(string text)
        {
            if (DateTime.TryParseExact(
                    text.Trim(),
                    DateFormats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime exact))
            {
                return exact;
            }

            return DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime parsed)
                ? parsed
                : null;
        }

        /// <summary>Строка вида "|---|---|" из markdown-таблицы.</summary>
        private static bool StartsWithWord(string value, params string[] words) =>
            words.Any(w => value.StartsWith(w, StringComparison.CurrentCultureIgnoreCase));

        /// <summary>Убирает знак сравнения и словесный префикс, оставляя число.</summary>
        private static string StripPrefix(string value, params char[] signs)
        {
            string trimmed = value.TrimStart(signs).TrimStart('=', ' ');
            int digit = trimmed.IndexOfAny("0123456789".ToCharArray());

            return digit < 0 ? trimmed : trimmed[digit..];
        }

        private static bool IsMarkdownRule(string line) =>
            line.Trim('|', '-', ':', ' ', '=').Length == 0;

        private static bool LooksLikeHeader(string[] parts) =>
            ParseNumber(Get(parts, 1)) is null &&
            parts.Any(p => HeaderWords.Any(w => p.Contains(w, StringComparison.CurrentCultureIgnoreCase)));

        private static string[] SplitRow(string line)
        {
            char separator = Separators.FirstOrDefault(line.Contains);

            if (separator == default)
            {
                return Array.Empty<string>();
            }

            return line
                .Trim(separator, ' ')
                .Split(separator)
                .Select(p => p.Trim())
                .ToArray();
        }

        private static double? ParseNumber(string? text) => ParseNumber(text, out _);

        /// <summary>
        /// Разбирает число из бланка. Формат заранее не известен: бланк немецкий, но текст
        /// приходит от чат-бота, а тот пишет и «1.234,5», и «1,234.5», и «1234.5».
        ///
        /// <paramref name="ambiguous"/> — запись, которую нельзя прочитать однозначно. Значение
        /// всё равно возвращается, иначе строка потеряется, но вызывающий обязан о нём
        /// предупредить: ошибка здесь стоит множителя в тысячу.
        /// </summary>
        private static double? ParseNumber(string? text, out bool ambiguous)
        {
            ambiguous = false;

            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            // Разделители разрядов, которые не являются ни точкой, ни запятой: пробелы
            // разных видов регулярно приходят из чатов и таблиц, апостроф — швейцарская запись.
            string value = text
                .Replace('\u00A0', ' ')
                .Replace('\u202F', ' ')
                .Replace('\u2009', ' ')
                .Replace(" ", string.Empty)
                .Replace("'", string.Empty)
                .Trim();

            int commas = value.Count(c => c == ',');
            int dots = value.Count(c => c == '.');

            if (commas > 0 && dots > 0)
            {
                // Оба знака сразу: десятичный — тот, что правее, второй разделяет разряды.
                value = value.LastIndexOf(',') > value.LastIndexOf('.')
                    ? value.Replace(".", string.Empty).Replace(',', '.')
                    : value.Replace(",", string.Empty);
            }
            else if (commas > 1 || dots > 1)
            {
                // Знак повторяется — десятичным он быть не может: «1.234.567».
                value = value.Replace(",", string.Empty).Replace(".", string.Empty);
            }
            else if (commas == 1)
            {
                // Запятая в бланке — всегда десятичный разделитель.
                value = value.Replace(',', '.');
            }
            else if (dots == 1)
            {
                value = ReadSingleDot(value, out ambiguous);
            }

            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed)
                ? parsed
                : null;
        }

        /// <summary>
        /// Одна точка и больше ничего. В немецком бланке точка разделяет разряды: «254.000» —
        /// это 254 000 тромбоцитов, а вовсе не 254. Прочти это по-английски — и в историю
        /// молча уедет число в тысячу раз меньше.
        ///
        /// По одному токену определить нельзя, поэтому решают две вещи: ровно три цифры справа
        /// (иначе разрядом быть не может) и целая часть от одной до трёх цифр без ведущего
        /// нуля: «0.123» — это дробь, а не разряды.
        /// </summary>
        private static string ReadSingleDot(string value, out bool ambiguous)
        {
            ambiguous = false;

            int dot = value.IndexOf('.');
            string head = value[..dot].TrimStart('+', '-');
            string tail = value[(dot + 1)..];

            bool looksGrouped =
                tail.Length == 3 && tail.All(char.IsAsciiDigit) &&
                head.Length is >= 1 and <= 3 && head.All(char.IsAsciiDigit) &&
                head[0] != '0';

            if (!looksGrouped)
            {
                return value;
            }

            ambiguous = true;

            return value.Replace(".", string.Empty);
        }

        private static string Get(string[] parts, int index) => index < parts.Length ? parts[index].Trim() : string.Empty;

        private static string? Empty(string value) => value.Length == 0 || value is "-" or "—" ? null : value;

        private static string Shorten(string line) => line.Length <= 40 ? line : line[..40] + "…";
    }
}
