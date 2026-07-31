using System.ComponentModel;
using System.Globalization;

namespace MedicalResultsTracker.Resources.Strings
{
    /// <summary>
    /// Источник строк для XAML и переключатель языка.
    /// Индексатор нужен, чтобы разметка подписывалась на изменение: при смене языка
    /// достаточно сообщить об обновлении индексатора, и все надписи перечитываются
    /// без перезапуска приложения.
    /// </summary>
    public sealed class Localization : INotifyPropertyChanged
    {
        private const string LanguageKey = "app.language";

        public static Localization Current { get; } = new();

        private Localization()
        {
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Языки, между которыми можно переключаться. Пустой код — «как в системе».</summary>
        public static IReadOnlyList<LanguageOption> Available { get; } = new[]
        {
            new LanguageOption(string.Empty, "System"),
            new LanguageOption("de", "Deutsch"),
            new LanguageOption("ru", "Русский"),
        };

        public string this[string key] => S.Get(key);

        /// <summary>Код выбранного языка или пустая строка, если язык берётся из системы.</summary>
        public string SelectedCode => Preferences.Default.Get(LanguageKey, string.Empty);

        /// <summary>
        /// Применяет сохранённый выбор. Вызывается до построения интерфейса,
        /// иначе первый экран успеет отрисоваться на языке системы.
        /// </summary>
        public void Restore() => Apply(SelectedCode);

        public void SetLanguage(string code)
        {
            Preferences.Default.Set(LanguageKey, code ?? string.Empty);

            Apply(code);

            // Пустая строка в имени свойства — «изменилось всё», включая индексатор.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedCode)));
        }

        private static void Apply(string code)
        {
            CultureInfo culture = string.IsNullOrEmpty(code)
                ? CultureInfo.InstalledUICulture
                : new CultureInfo(code);

            CultureInfo.DefaultThreadCurrentUICulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }
    }

    /// <summary>Пункт списка языков.</summary>
    /// <param name="Code">Код культуры; пустой — язык системы.</param>
    /// <param name="Name">Название на самом этом языке, а не в переводе.</param>
    public sealed record LanguageOption(string Code, string Name);
}
