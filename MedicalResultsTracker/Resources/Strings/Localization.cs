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
        private const string DefaultCode = "de";

        public static Localization Current { get; } = new();

        private Localization()
        {
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Языки, между которыми можно переключаться. Пустой код — «как в системе».</summary>
        public static IReadOnlyList<LanguageOption> Available { get; } = new[]
        {
            new LanguageOption(string.Empty, "Lang_System"),
            new LanguageOption("de", "Deutsch"),
            new LanguageOption("ru", "Русский"),
        };

        public string this[string key] => S.Get(key);

        /// <summary>
        /// Код выбранного языка. По умолчанию немецкий, а не системный: приложение делается
        /// для немецкого рынка, и на любом телефоне оно должно открываться по-немецки,
        /// пока пользователь не выберет другое.
        /// </summary>
        public string SelectedCode => Preferences.Default.Get(LanguageKey, DefaultCode);

        /// <summary>
        /// Применяет сохранённый выбор. Вызывается до построения интерфейса,
        /// иначе первый экран успеет отрисоваться на языке системы.
        /// </summary>
        public void Restore() => Apply(SelectedCode);

        public void SetLanguage(string code)
        {
            Preferences.Default.Set(LanguageKey, code ?? string.Empty);

            Apply(code);

            // Пустое имя — «изменилось всё»; "Item[]" — соглашение для индексатора.
            // Шлём оба: какое из них поймёт привязка, зависит от версии MAUI.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
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
    /// <param name="NameOrKey">Название на самом этом языке. Для «как в системе» —
    /// ключ ресурса: у этого пункта своего языка нет, он должен переводиться.</param>
    public sealed record LanguageOption(string Code, string NameOrKey)
    {
        public string Name => S.Find(NameOrKey) ?? NameOrKey;
    }
}
