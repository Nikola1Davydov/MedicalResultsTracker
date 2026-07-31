using MedicalResultsTracker.View;

namespace MedicalResultsTracker.ViewModel
{
    internal static class ViewModelRegistrator
    {
        public static IServiceCollection RegisterViewModels(this IServiceCollection services) => services
            .AddSingleton<MainViewModel>()
            .AddSingleton<MatrixViewModel>()
            .AddSingleton<TrendsViewModel>()
            .AddSingleton<SettingsViewModel>()

            // Экраны с параметром маршрута создаются заново на каждый переход.
            .AddTransient<HistoryViewModel>()
            .AddTransient<TestEditViewModel>()
            .AddTransient<TrendDetailViewModel>()
            .AddTransient<CatalogViewModel>()
            .AddTransient<CatalogEditViewModel>()
            .AddTransient<ViewEditViewModel>()
            ;

        public static IServiceCollection RegisterPages(this IServiceCollection services) => services
            .AddSingleton<MainPage>()
            .AddSingleton<MatrixPage>()
            .AddSingleton<TrendsPage>()
            .AddSingleton<SettingsPage>()
            .AddTransient<HistoryPage>()
            .AddTransient<TestEditPage>()
            .AddTransient<TrendDetailPage>()
            .AddTransient<CatalogPage>()
            .AddTransient<CatalogEditPage>()
            .AddTransient<ViewEditPage>()
            ;
    }
}
