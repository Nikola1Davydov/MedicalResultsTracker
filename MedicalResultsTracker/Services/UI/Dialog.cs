using MedicalResultsTracker.Resources.Strings;
namespace MedicalResultsTracker.Services.UI
{
    /// <summary>
    /// Единственное место, где приложение показывает системные диалоги.
    /// Собрано в одну точку намеренно: API диалогов в MAUI менялся от версии к версии,
    /// и правка нужна будет здесь, а не в каждой ViewModel.
    /// </summary>
    internal static class Dialog
    {
        public static Task AlertAsync(string title, string message) =>
            CurrentPage is Page page ? page.DisplayAlert(title, message, S.Common_Ok) : Task.CompletedTask;

        /// <param name="cancel">По умолчанию — локализованная «Отмена»; значением параметра
        /// ресурс быть не может, поэтому подставляется в теле.</param>
        public static Task<bool> ConfirmAsync(string title, string message, string accept, string? cancel = null) =>
            CurrentPage is Page page
                ? page.DisplayAlert(title, message, accept, cancel ?? S.Common_Cancel)
                : Task.FromResult(false);

        private static Page? CurrentPage => Application.Current?.Windows.FirstOrDefault()?.Page;
    }
}
