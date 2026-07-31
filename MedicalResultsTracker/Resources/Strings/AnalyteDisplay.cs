using MedicalResultsTracker.Model;

namespace MedicalResultsTracker.Resources.Strings
{
    /// <summary>
    /// Перевод того, что хранится в базе, — на этапе вывода.
    ///
    /// Группы встроенного справочника переводятся: это элемент интерфейса, и в английской
    /// версии заголовок «Blutbild» был бы просто непонятен. Названия самих показателей
    /// не переводятся намеренно: они повторяют то, что напечатано в немецком бланке,
    /// и пользователь сверяет их глазами со своей бумагой.
    /// </summary>
    public static class AnalyteDisplay
    {
        /// <summary>Название группы для экрана. Придуманные пользователем возвращаются как есть.</summary>
        public static string Category(string? stored)
        {
            if (string.IsNullOrWhiteSpace(stored))
            {
                return S.Trend_NoGroup;
            }

            return S.Find(AnalyteCategories.ResourceKey(stored)) ?? stored.Trim();
        }
    }
}
