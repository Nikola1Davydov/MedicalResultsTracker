using System.Globalization;

namespace MedicalResultsTracker.Services.Ai
{
    /// <inheritdoc cref="IAiConsentService"/>
    public sealed class AiConsentService : IAiConsentService
    {
        private const string ScopeKey = "ai.consent.scope";
        private const string ProviderKey = "ai.consent.provider";
        private const string GrantedKey = "ai.consent.granted_utc";

        private AiConsent _current;

        public AiConsentService()
        {
            _current = Load();
        }

        public event EventHandler<AiConsent>? Changed;

        public AiConsent Current => _current;

        public bool IsAllowed(AiConsentScope scope) => _current.Allows(scope);

        public void Grant(AiConsentScope scope, string provider)
        {
            // Смена провайдера обнуляет ранее выданные разрешения: согласие всегда привязано к получателю данных.
            AiConsentScope baseScope = string.Equals(_current.Provider, provider, StringComparison.Ordinal)
                ? _current.Scope
                : AiConsentScope.None;

            Save(new AiConsent
            {
                Scope = baseScope | scope,
                Provider = provider,
                GrantedUtc = DateTime.UtcNow,
            });
        }

        public void Revoke(AiConsentScope scope)
        {
            AiConsentScope remaining = _current.Scope & ~scope;

            if (remaining == AiConsentScope.None)
            {
                RevokeAll();
                return;
            }

            Save(new AiConsent
            {
                Scope = remaining,
                Provider = _current.Provider,
                GrantedUtc = _current.GrantedUtc,
            });
        }

        public void RevokeAll() => Save(AiConsent.None);

        private void Save(AiConsent consent)
        {
            _current = consent;

            Preferences.Default.Set(ScopeKey, (int)consent.Scope);
            Preferences.Default.Set(ProviderKey, consent.Provider ?? string.Empty);
            Preferences.Default.Set(
                GrantedKey,
                consent.GrantedUtc?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty);

            Changed?.Invoke(this, consent);
        }

        private static AiConsent Load()
        {
            AiConsentScope scope = (AiConsentScope)Preferences.Default.Get(ScopeKey, 0);

            if (scope == AiConsentScope.None)
            {
                return AiConsent.None;
            }

            string provider = Preferences.Default.Get(ProviderKey, string.Empty);
            string granted = Preferences.Default.Get(GrantedKey, string.Empty);

            return new AiConsent
            {
                Scope = scope,
                Provider = string.IsNullOrEmpty(provider) ? null : provider,
                GrantedUtc = DateTime.TryParse(
                    granted,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out DateTime parsed)
                    ? parsed
                    : null,
            };
        }
    }
}
