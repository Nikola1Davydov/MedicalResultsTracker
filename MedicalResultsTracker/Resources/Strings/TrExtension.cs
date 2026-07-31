using Microsoft.Maui.Controls.Xaml;

namespace MedicalResultsTracker.Resources.Strings
{
    /// <summary>
    /// Разметка для XAML: <c>Text="{loc:Tr Dash_Title}"</c>.
    /// Возвращает не строку, а привязку к <see cref="Localization"/>: иначе смена языка
    /// в настройках потребовала бы перезапуска приложения.
    /// </summary>
    [ContentProperty(nameof(Key))]
    [AcceptEmptyServiceProvider]
    public sealed class TrExtension : IMarkupExtension<BindingBase>
    {
        public string Key { get; set; } = string.Empty;

        public BindingBase ProvideValue(IServiceProvider serviceProvider) => new Binding
        {
            Mode = BindingMode.OneWay,
            Path = $"[{Key}]",
            Source = Localization.Current,
        };

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
    }
}
