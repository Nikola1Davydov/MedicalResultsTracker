namespace MedicalResultsTracker
{
    /// <summary>Маршруты Shell в одном месте, чтобы не разъезжались строки в разных ViewModel.</summary>
    public static class AppRoutes
    {
        public const string Dashboard = "//dashboard";
        public const string Matrix = "//matrix";
        public const string History = "//history";
        public const string Trends = "//trends";
        public const string Settings = "//settings";

        public const string TestEdit = "testedit";
        public const string TrendDetail = "trenddetail";
        public const string Catalog = "catalog";
        public const string CatalogEdit = "catalogedit";
        public const string ViewEdit = "viewedit";

        public const string TestIdParameter = "testId";
        public const string SeriesKeyParameter = "seriesKey";
        public const string AnalyteCodeParameter = "analyteCode";
        public const string ViewIdParameter = "viewId";
    }
}
