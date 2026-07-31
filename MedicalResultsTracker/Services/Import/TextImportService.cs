using System.Globalization;
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

        public string PromptForChat =>
            """
            Unten ist das Foto eines Laborbefunds. Übertrage die Daten daraus als reinen Text
            genau in diesem Format und ergänze nichts von dir aus.

            Die ersten beiden Zeilen:
            Datum: TT.MM.JJJJ
            Labor: Name oder Strich

            Danach eine Zeile je Wert, die Felder mit einem senkrechten Strich getrennt:
            Bezeichnung | Ergebnis | Einheit | Referenz von | Referenz bis

            Regeln:
            - Übernimm die Bezeichnungen genau wie im Befund: nicht übersetzen, nicht abkürzen,
              nicht umbenennen.
            - Das Ergebnis ist nur eine Zahl. Ist es nicht numerisch, schreibe es als Wort
              («negativ», «Spuren»).
            - Der Referenzbereich sind zwei getrennte Zahlen. Ist er einseitig («bis 5,2»),
              fülle nur das passende Feld, das andere bleibt leer.
            - Rate nichts und rechne nichts um: Ist ein Feld im Befund nicht lesbar, lass es leer.
            - Keine Tabellenüberschrift, keine Nummerierung, keine Erläuterungen, keine
              Schlussfolgerungen und keine Empfehlungen. Nur die Datenzeilen.
            """;

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

                AiDraftRow? row = ParseRow(parts);

                if (row is null)
                {
                    draft.Warnings.Add(string.Format(S.Imp_SkippedLine, Shorten(line)));
                    continue;
                }

                draft.Rows.Add(row);
            }

            if (draft.Rows.Count == 0)
            {
                draft.Warnings.Add(S.Imp_NoRows);
            }

            return draft;
        }

        private static AiDraftRow? ParseRow(string[] parts)
        {
            string name = parts[0].Trim();

            if (name.Length == 0)
            {
                return null;
            }

            string valueText = Get(parts, 1);
            double? value = ParseNumber(valueText);

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

        private static double? ParseNumber(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            // Неразрывный пробел приходит из чатов и таблиц регулярно: «1 234,5».
            string normalized = text
                .Replace(',', '.')
                .Replace('\u00A0', ' ')
                .Replace(" ", string.Empty)
                .Replace(" ", string.Empty)
                .Trim();

            return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out double value)
                ? value
                : null;
        }

        private static string Get(string[] parts, int index) => index < parts.Length ? parts[index].Trim() : string.Empty;

        private static string? Empty(string value) => value.Length == 0 || value is "-" or "—" ? null : value;

        private static string Shorten(string line) => line.Length <= 40 ? line : line[..40] + "…";
    }
}
