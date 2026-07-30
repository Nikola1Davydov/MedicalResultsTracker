namespace MedicalResultsTracker.Services.Ai
{
    /// <summary>
    /// Необязательная надстройка над приложением. Всё, что она делает, пользователь может сделать руками.
    /// Реализация обязана проверять <see cref="IAiConsentService"/> перед любой передачей данных наружу
    /// и обязана возвращать черновик, а не писать в базу напрямую.
    /// </summary>
    public interface IAiAssistant
    {
        /// <summary>Человекочитаемое имя получателя данных — показываем его в окне согласия.</summary>
        string ProviderName { get; }

        /// <summary>true, только если провайдер настроен И согласие на нужную операцию выдано.</summary>
        bool IsAvailable(AiConsentScope scope);

        /// <summary>Распознаёт таблицу результатов на фото или в PDF и возвращает черновик для проверки.</summary>
        Task<AiDraft?> ExtractAsync(string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Текстовое пояснение к динамике выбранных показателей.
        /// Это справочный текст, а не медицинская рекомендация.
        /// </summary>
        Task<string?> ExplainAsync(IReadOnlyList<Model.ParameterTrend> trends, CancellationToken cancellationToken = default);
    }
}
