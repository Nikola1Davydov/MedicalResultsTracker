using MedicalResultsTracker.Controls;

namespace MedicalResultsTracker.ViewModel
{
    /// <summary>
    /// Кнопка-фильтр, которая видно когда включена.
    ///
    /// Раньше на каждый фильтр заводилось по три свойства во ViewModel — текст, заливка,
    /// цвет текста, — и добавить четвёртый фильтр значило дописать двенадцать. Здесь это
    /// один объект на фильтр, а экран рисует их списком.
    /// </summary>
    public sealed partial class FilterChipViewModel : ObservableObject
    {
        private readonly string _label;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Text))]
        [NotifyPropertyChangedFor(nameof(Background))]
        [NotifyPropertyChangedFor(nameof(Foreground))]
        private bool _isActive;

        public FilterChipViewModel(string label, object parameter, bool isActive = false)
        {
            _label = label;
            Parameter = parameter;
            _isActive = isActive;
        }

        /// <summary>Что этот фильтр означает: значение перечисления либо имя переключателя.</summary>
        public object Parameter { get; }

        // Одной заливки мало: цвет прочитается не всеми. Галочка в тексте говорит то же самое словом.
        public string Text => IsActive ? $"✓ {_label}" : _label;

        public Color Background => IsActive ? StatusPalette.FilterOn : StatusPalette.FilterOff;

        public Color Foreground => IsActive ? Colors.White : StatusPalette.FilterOffText;
    }
}
