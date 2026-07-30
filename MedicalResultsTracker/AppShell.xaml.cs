using MedicalResultsTracker.View;

namespace MedicalResultsTracker
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Экраны, на которые переходят с параметром, вкладками не являются.
            Routing.RegisterRoute(AppRoutes.TestEdit, typeof(TestEditPage));
            Routing.RegisterRoute(AppRoutes.TrendDetail, typeof(TrendDetailPage));
        }
    }
}
