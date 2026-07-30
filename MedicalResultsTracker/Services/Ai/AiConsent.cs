namespace MedicalResultsTracker.Services.Ai
{
    /// <summary>
    /// На что именно дано согласие. Флаги раздельные: разрешить распознавание бланка
    /// не значит разрешить отправлять историю на комментарий.
    /// </summary>
    [Flags]
    public enum AiConsentScope
    {
        None = 0,

        /// <summary>Разовая отправка одного изображения/PDF для распознавания таблицы.</summary>
        DocumentRecognition = 1,

        /// <summary>Отправка выбранных показателей, чтобы получить текстовое пояснение.</summary>
        ResultCommentary = 2
    }

    /// <summary>Что именно пользователь разрешил и когда. Хранится локально.</summary>
    public sealed class AiConsent
    {
        public static AiConsent None { get; } = new();

        public AiConsentScope Scope { get; init; } = AiConsentScope.None;

        /// <summary>Имя провайдера, на который дано согласие. Смена провайдера сбрасывает согласие.</summary>
        public string? Provider { get; init; }

        public DateTime? GrantedUtc { get; init; }

        public bool Allows(AiConsentScope scope) => scope != AiConsentScope.None && Scope.HasFlag(scope);
    }
}
