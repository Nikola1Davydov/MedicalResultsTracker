namespace MedicalResultsTracker.Model
{
    /// <summary>
    /// Как название показателя превращается в код. Правило одно на всё приложение:
    /// по нему и заводятся новые записи каталога, и склеиваются измерения между анализами.
    /// Стоит двум местам разойтись — и один и тот же показатель распадётся на два графика.
    /// </summary>
    public static class AnalyteCode
    {
        private const int MaxLength = 48;

        public static string FromName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            char[] normalized = name.Trim().ToUpperInvariant()
                .Select(c => char.IsLetterOrDigit(c) ? c : '_')
                .ToArray();

            string code = new(normalized);

            return code.Length > MaxLength ? code[..MaxLength] : code;
        }

        /// <summary>Ключ, по которому измерение сопоставляется с измерениями из других анализов.</summary>
        public static string KeyOf(string? code, string? name) =>
            string.IsNullOrWhiteSpace(code) ? FromName(name) : code.Trim().ToUpperInvariant();
    }
}
