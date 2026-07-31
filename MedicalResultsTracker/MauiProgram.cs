using MedicalResultsTracker.Resources.Strings;
using MedicalResultsTracker.Services;
using MedicalResultsTracker.ViewModel;
using Microsoft.Extensions.Logging;

namespace MedicalResultsTracker
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            // До построения интерфейса: иначе первый экран отрисуется на языке системы,
            // даже если пользователь выбрал другой.
            Localization.Current.Restore();

            MauiAppBuilder builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.RegisterServices();
            builder.Services.RegisterViewModels();
            builder.Services.RegisterPages();
#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
