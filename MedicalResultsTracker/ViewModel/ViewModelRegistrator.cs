namespace MedicalResultsTracker.ViewModel
{
    internal static class ViewModelRegistrator
    {
        public static void RegisterViewModels(this IServiceCollection services) => services
            .AddSingleton<MainViewModel>()
            ;

    }
}
