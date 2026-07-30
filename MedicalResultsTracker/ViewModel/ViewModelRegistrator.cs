using MedicalResultsTracker.View;

namespace MedicalResultsTracker.ViewModel
{
    internal static class ViewModelRegistrator
    {
        public static IServiceCollection RegisterViewModels(this IServiceCollection services) => services
            .AddSingleton<MainViewModel>()
            .AddSingleton<HistoryViewModel>()
            .AddSingleton<TrendsViewModel>()
            .AddSingleton<SettingsViewModel>()

            // Экраны с параметром маршрута создаются заново на каждый переход.
            .AddTransient<TestEditViewModel>()
            .AddTransient<TrendDetailViewModel>()
            .AddTransient<CatalogViewModel>()
            .AddTransient<CatalogEditViewModel>()
            ;

        public static IServiceCollection RegisterPages(this IServiceCollection services) => services
            .AddSingleton<MainPage>()
            .AddSingleton<HistoryPage>()
            .AddSingleton<TrendsPage>()
            .AddSingleton<SettingsPage>()
            .AddTransient<TestEditPage>()
            .AddTransient<TrendDetailPage>()
            .AddTransient<CatalogPage>()
            .AddTransient<CatalogEditPage>()
            ;
    }
}
