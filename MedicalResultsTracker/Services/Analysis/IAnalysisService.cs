using MedicalResultsTracker.Model;

namespace MedicalResultsTracker.Services.Analysis
{
    /// <summary>
    /// Локальные расчёты по истории: сравнение с прошлым разом и ряды для диаграмм.
    /// Никакой интерпретации "что это значит для здоровья" — только арифметика и границы из бланка.
    /// </summary>
    public interface IAnalysisService
    {
        /// <summary>Показатели последнего анализа в сравнении с предыдущим по дате.</summary>
        Task<IReadOnlyList<ParameterTrend>> GetLatestTrendsAsync();

        /// <summary>Сравнение двух конкретных анализов.</summary>
        Task<IReadOnlyList<ParameterTrend>> CompareAsync(Guid currentTestId, Guid? previousTestId = null);

        /// <summary>Ряды всех показателей, по которым есть хотя бы одно числовое значение.</summary>
        Task<IReadOnlyList<ParameterSeries>> GetSeriesAsync();

        Task<ParameterSeries?> GetSeriesAsync(string key);

        /// <summary>
        /// Сводная таблица: строка — показатель, столбец — дата. Один столбец на календарную дату,
        /// сколько бы бланков за это число ни было записано.
        /// </summary>
        Task<ResultMatrix> BuildMatrixAsync();

        /// <summary>Ключ группировки показателя между анализами: код из каталога либо нормализованное название.</summary>
        string GetKey(BloodParameter parameter);
    }
}
