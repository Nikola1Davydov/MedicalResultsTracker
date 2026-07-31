using MedicalResultsTracker.Services.Ai;
using MedicalResultsTracker.Services.Analysis;
using MedicalResultsTracker.Services.Database;
using MedicalResultsTracker.Services.Export;
using MedicalResultsTracker.Services.Import;
using MedicalResultsTracker.Services.UI;

namespace MedicalResultsTracker.Services
{
    internal static class ServicesRegistrator
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services) => services
            .AddSingleton<IMedicalDatabase, MedicalDatabase>()
            .AddSingleton<IBloodTestRepository, BloodTestRepository>()
            .AddSingleton<IAnalyteCatalog, AnalyteCatalog>()
            .AddSingleton<IMatrixViewRepository, MatrixViewRepository>()
            .AddSingleton<IAnalysisService, AnalysisService>()
            .AddSingleton<IExportService, ExportService>()
            .AddSingleton<ITextImportService, TextImportService>()
            .AddSingleton<IAiConsentService, AiConsentService>()

            // Связь между XAML-экранами и таблицей, которая живёт в веб-слое.
            .AddSingleton<MatrixState>()

            // Заглушка: пока внешний провайдер не подключён, приложение работает полностью локально.
            .AddSingleton<IAiAssistant, DisabledAiAssistant>()
            ;
    }
}
