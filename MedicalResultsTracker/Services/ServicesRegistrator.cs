using MedicalResultsTracker.Services.Ai;
using MedicalResultsTracker.Services.Analysis;
using MedicalResultsTracker.Services.Database;
using MedicalResultsTracker.Services.Export;
using MedicalResultsTracker.Services.Import;

namespace MedicalResultsTracker.Services
{
    internal static class ServicesRegistrator
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services) => services
            .AddSingleton<IMedicalDatabase, MedicalDatabase>()
            .AddSingleton<IBloodTestRepository, BloodTestRepository>()
            .AddSingleton<IAnalyteCatalog, AnalyteCatalog>()
            .AddSingleton<IAnalysisService, AnalysisService>()
            .AddSingleton<IExportService, ExportService>()
            .AddSingleton<ITextImportService, TextImportService>()
            .AddSingleton<IAiConsentService, AiConsentService>()

            // Заглушка: пока внешний провайдер не подключён, приложение работает полностью локально.
            .AddSingleton<IAiAssistant, DisabledAiAssistant>()
            ;
    }
}
