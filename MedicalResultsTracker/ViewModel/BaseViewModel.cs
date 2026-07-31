using MedicalResultsTracker.Services.UI;
using MedicalResultsTracker.Resources.Strings;

namespace MedicalResultsTracker.ViewModel
{
    public abstract partial class BaseViewModel : ObservableObject
    {
        /// <summary>
        /// Защёлка от второго параллельного запуска. Намеренно отдельно от <see cref="IsBusy"/>:
        /// <c>IsBusy</c> привязан к <c>RefreshView.IsRefreshing</c>, а тот по умолчанию TwoWay
        /// и при жесте «потянуть вниз» сам выставляет его в true — ещё до вызова команды.
        /// Держи защёлку на нём — команда сочла бы себя уже запущенной, вышла бы, не сделав ничего,
        /// и сбросить индикатор стало бы некому: крутилка навсегда, экран больше не обновляется.
        ///
        /// Поле, а не свойство: все переходы происходят в UI-потоке, синхронизация не нужна.
        /// </summary>
        private bool _isRunning;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _title = string.Empty;

        /// <summary>Вызывается страницей при появлении на экране.</summary>
        public virtual Task InitializeAsync() => Task.CompletedTask;

        /// <summary>Выполняет операцию, не давая запустить вторую параллельно, и не роняет UI на ошибке.</summary>
        protected async Task RunAsync(Func<Task> operation, string? errorTitle = null)
        {
            if (_isRunning)
            {
                return;
            }

            _isRunning = true;
            IsBusy = true;

            Exception? failure = null;

            try
            {
                await operation().ConfigureAwait(true);
            }
            catch (Exception exception)
            {
                failure = exception;
            }
            finally
            {
                _isRunning = false;
                IsBusy = false;
            }

            // Диалог показывается после снятия индикатора: зависни он (а системный диалог
            // на Android умеет зависнуть, если предыдущий ещё не закрыт) — экран остался бы
            // заблокированным насовсем.
            if (failure is not null)
            {
                Debug.WriteLine($"[MedicalResultsTracker] {errorTitle ?? S.Common_Error}: {failure}");

                await Dialog.AlertAsync(errorTitle ?? S.Common_Error, failure.Message);
            }
        }
    }
}
