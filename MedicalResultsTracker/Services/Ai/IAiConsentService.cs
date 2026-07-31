namespace MedicalResultsTracker.Services.Ai
{
    /// <summary>
    /// Единственная точка, где решается, можно ли вообще обращаться к ИИ.
    /// По умолчанию запрещено всё; согласие даётся явно и в любой момент отзывается.
    /// </summary>
    public interface IAiConsentService
    {
        AiConsent Current { get; }

        bool IsAllowed(AiConsentScope scope);

        void Grant(AiConsentScope scope, string provider);

        void Revoke(AiConsentScope scope);

        void RevokeAll();

        event EventHandler<AiConsent>? Changed;
    }
}
