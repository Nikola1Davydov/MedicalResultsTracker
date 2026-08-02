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

        /// <summary>
        /// Список действий на выбор. Нужен там, где вариантов больше двух и они ещё будут
        /// добавляться: список растёт вниз, ничего не переставляя на экране, — в отличие
        /// от ряда кнопок, который на телефоне упирается в ширину уже на четвёртой.
        /// </summary>
        /// <returns>Выбранный пункт либо null, если человек закрыл список.</returns>
        public static async Task<string?> ChooseAsync(string title, params string[] options)
        {
            if (CurrentPage is not Page page || options.Length == 0)
            {
                return null;
            }

            string? chosen = await page.DisplayActionSheet(title, S.Common_Cancel, null, options);

            // Отмена возвращается тем же способом, что и выбор, — по тексту кнопки.
            return chosen is null || chosen == S.Common_Cancel ? null : chosen;
        }

        private static Page? CurrentPage => Application.Current?.Windows.FirstOrDefault()?.Page;
    }
}
