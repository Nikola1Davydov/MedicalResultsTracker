using MedicalResultsTracker.Model;
using MedicalResultsTracker.Resources.Strings;

namespace MedicalResultsTracker.Services.Ai
{
    /// <summary>
    /// Реализация по умолчанию: ассистента нет.
    /// Приложение собрано так, что в нём физически отсутствует код обращения к внешнему сервису —
    /// сначала архитектура и согласие, только потом подключение (см. docs/AI-ASSISTANT.md).
    /// </summary>
    public sealed class DisabledAiAssistant : IAiAssistant
    {
        public string ProviderName => S.Ai_NotConnected;

        public bool IsAvailable(AiConsentScope scope) => false;

        public Task<AiDraft?> ExtractAsync(string filePath, CancellationToken cancellationToken = default) =>
            Task.FromResult<AiDraft?>(null);

        public Task<string?> ExplainAsync(
            IReadOnlyList<ParameterTrend> trends,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<string?>(null);
    }
}
