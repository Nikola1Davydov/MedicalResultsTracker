namespace MedicalResultsTracker.Services.UI
{
    /// <summary>
    /// Общее состояние сводной таблицы. Нужно потому, что сама таблица живёт в веб-слое
    /// (Blazor), а открывают и закрывают её экраны на XAML: напрямую дотянуться друг до друга
    /// они не могут, и связь идёт через эту небольшую прослойку.
    /// </summary>
    public sealed class MatrixState
    {
        /// <summary>
        /// Набор, который нужно показать при следующем построении таблицы.
        /// Ставится редактором сразу после сохранения: человек только что собрал набор — он и должен открыться.
        /// </summary>
        public Guid? PendingViewId { get; set; }

        /// <summary>Экран появился — таблице пора перечитать данные.</summary>
        public event Action? RefreshRequested;

        public void RequestRefresh() => RefreshRequested?.Invoke();
    }
}
