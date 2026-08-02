using System.Globalization;

namespace MedicalResultsTracker.Model
{
    /// <summary>
    /// Чтение чисел из бланка анализа — **единственное** место, где это делается.
    ///
    /// Раньше реализаций было две: одна для текста, вставленного из чат-бота, другая для
    /// ручного ввода. Они разошлись, и один и тот же бланк давал разные числа: «254.000»
    /// читалось как 254 000 при вставке и как 254 при наборе руками. В медицинской записи
    /// такое расхождение стоит дороже любого удобства раздельных реализаций.
    /// </summary>
    public static class LabNumber
    {
        /// <summary>Число из строки. Неоднозначность игнорируется — для мест, где предупредить негде.</summary>
        public static double? Parse(string? text) => Parse(text, out _);

        /// <summary>
        /// Разбирает число из бланка. Формат заранее не известен: бланк немецкий, но текст
        /// приходит от чат-бота, а тот пишет и «1.234,5», и «1,234.5», и «1234.5».
        ///
        /// <paramref name="ambiguous"/> — запись, которую нельзя прочитать однозначно. Значение
        /// всё равно возвращается, иначе строка потеряется, но вызывающий обязан о нём
        /// предупредить: ошибка здесь стоит множителя в тысячу.
        /// </summary>
        public static double? Parse(string? text, out bool ambiguous)
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
    }
}
