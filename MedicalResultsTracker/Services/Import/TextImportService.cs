using System.Globalization;
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
            "показатель", "название", "значение", "результат", "единиц", "норма", "референс"
        };

        public string PromptForChat =>
            """
            Ниже фотография бланка лабораторных анализов. Перепиши данные из него обычным
            текстом строго по этому формату и ничего не добавляй от себя.

            Первые две строки:
            Дата: дд.мм.гггг
            Лаборатория: название или прочерк

            Дальше по одной строке на каждый показатель, поля разделены вертикальной чертой:
            Название | Значение | Единицы | Норма от | Норма до

            Правила:
            - Названия показателей оставляй ровно как в бланке: не переводи, не сокращай,
              не переименовывай.
            - Значение — только число. Если результат нечисловой, напиши его словом
              («отрицательно», «следы»).
            - Границы нормы — два отдельных числа. Если норма односторонняя («до 5,2»),
              заполни только нужное поле, второе оставь пустым.
            - Ничего не додумывай и не пересчитывай: если поле в бланке не читается,
              оставь его пустым.
            - Не добавляй заголовок таблицы, нумерацию, пояснения, выводы и рекомендации.
              Нужны только строки данных.
            """;

        public AiDraft Parse(string text)
        {
            AiDraft draft = new();

            if (string.IsNullOrWhiteSpace(text))
            {
                draft.Warnings.Add("Пустой текст.");
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

                if (TryReadHeaderField(line, "дата", out string dateValue))
                {
                    draft.Date = ParseDate(dateValue);

                    if (draft.Date is null)
                    {
                        draft.Warnings.Add($"Не разобрана дата: «{dateValue}».");
                    }

                    continue;
                }

                if (TryReadHeaderField(line, "лаборатория", out string lab))
                {
                    draft.Laboratory = lab is "-" or "—" or "" ? null : lab;
                    continue;
                }

                string[] parts = SplitRow(line);

                if (parts.Length < 2)
                {
                    draft.Warnings.Add($"Пропущена строка: «{Shorten(line)}».");
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
                    draft.Warnings.Add($"Пропущена строка: «{Shorten(line)}».");
                    continue;
                }

                draft.Rows.Add(row);
            }

            if (draft.Rows.Count == 0)
            {
                draft.Warnings.Add("Не найдено ни одной строки показателей.");
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
                value.StartsWith("до", StringComparison.CurrentCultureIgnoreCase))
            {
                return (null, ParseNumber(value.TrimStart('<', '≤', '=', ' ').TrimStart("до".ToCharArray())));
            }

            if (value.StartsWith('>') || value.StartsWith('≥') ||
                value.StartsWith("от", StringComparison.CurrentCultureIgnoreCase))
            {
                return (ParseNumber(value.TrimStart('>', '≥', '=', ' ').TrimStart("от".ToCharArray())), null);
            }

            string[] bounds = value.Split('–', '—', '-', '…');

            if (bounds.Length == 2)
            {
                return (ParseNumber(bounds[0]), ParseNumber(bounds[1]));
            }

            return (null, null);
        }

        private static bool TryReadHeaderField(string line, string field, out string value)
        {
            value = string.Empty;

            int colon = line.IndexOf(':');

            if (colon <= 0)
            {
                return false;
            }

            string key = line[..colon].Trim();

            if (!key.StartsWith(field, StringComparison.CurrentCultureIgnoreCase))
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
