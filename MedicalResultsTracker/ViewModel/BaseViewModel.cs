using MedicalResultsTracker.Services.UI;
using MedicalResultsTracker.Resources.Strings;

namespace MedicalResultsTracker.ViewModel
{
    public abstract partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _title = string.Empty;

        /// <summary>Вызывается страницей при появлении на экране.</summary>
        public virtual Task InitializeAsync() => Task.CompletedTask;

        /// <summary>Выполняет операцию, не давая запустить вторую параллельно, и не роняет UI на ошибке.</summary>
        protected async Task RunAsync(Func<Task> operation, string? errorTitle = null)
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                await operation().ConfigureAwait(true);
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"[MedicalResultsTracker] {errorTitle ?? S.Common_Error}: {exception}");

                await Dialog.AlertAsync(errorTitle ?? S.Common_Error, exception.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
