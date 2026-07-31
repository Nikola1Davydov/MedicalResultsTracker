using Microsoft.Maui.Controls.Xaml;

namespace MedicalResultsTracker.Resources.Strings
{
    /// <summary>
    /// Разметка для XAML: <c>Text="{loc:Tr Dash_Title}"</c>.
    /// Язык определяется системной локалью при запуске — переключателя внутри приложения нет.
    /// </summary>
    [ContentProperty(nameof(Key))]
    [AcceptEmptyServiceProvider]
    public sealed class TrExtension : IMarkupExtension<string>
    {
        public string Key { get; set; } = string.Empty;

        public string ProvideValue(IServiceProvider serviceProvider) => S.Get(Key);

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
    }
}
