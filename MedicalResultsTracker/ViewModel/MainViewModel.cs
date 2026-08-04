using System.Globalization;
using MedicalResultsTracker.Controls;
using MedicalResultsTracker.Model;
using MedicalResultsTracker.Resources.Strings;
using MedicalResultsTracker.Services.Analysis;
using MedicalResultsTracker.Services.Backup;
using MedicalResultsTracker.Services.Database;
using MedicalResultsTracker.Services.UI;

namespace MedicalResultsTracker.ViewModel
{
    /// <summary>Главный экран: что было в последний раз и что изменилось.</summary>
    public partial class MainViewModel : BaseViewModel
    {
        private readonly IBloodTestRepository _repository;
        private readonly IBloodPressureRepository _pressure;
        private readonly IAutoBackupService _backup;
        private readonly IAnalysisService _analysis;

        [ObservableProperty]
        private string _lastTestTitle = S.Dash_NoTests;

        [ObservableProperty]
        private string _summary = S.Dash_EmptyHint;

        [ObservableProperty]
        private bool _hasData;

        [ObservableProperty]
        private bool _isEmpty = true;

        /// <summary>
        /// Сводка вместо списка: два числа — сколько выше нормы и сколько ниже. Развёрнутый
        /// список тех же показателей живёт во вкладке «Динамика», где его можно отобрать
        /// фильтрами, — на главном экране он занимал полтора экрана прокрутки и повторял её.
        /// </summary>
        [ObservableProperty]
        private int _highCount;

        [ObservableProperty]
        private int _lowCount;

        [ObservableProperty]
        private bool _allInRange;

        /// <summary>Последнее измерение давления — «130/85 · сегодня, 08:20» либо приглашение начать.</summary>
        [ObservableProperty]
        private string _pressureSummary = string.Empty;

        [ObservableProperty]
        private Color _pressureColor = StatusPalette.Unknown;

        /// <summary>За один запуск копия делается один раз, а не на каждое открытие вкладки.</summary>
        private bool _backupChecked;

        public MainViewModel(
            IBloodTestRepository repository,
            IBloodPressureRepository pressure,
            IAutoBackupService backup,
            IAnalysisService analysis)
        {
            _repository = repository;
            _pressure = pressure;
            _backup = backup;
            _analysis = analysis;

            Title = S.Dash_Title;
        }

        public override Task InitializeAsync() => RunAsync(LoadAsync, S.Err_Load);

        [RelayCommand]
        private Task Refresh() => RunAsync(LoadAsync, S.Err_Refresh);

        [RelayCommand]
        private Task AddTest() => Shell.Current.GoToAsync(AppRoutes.TestEdit);

        /// <summary>Давление записывают ежедневно, поэтому кнопка ведёт сразу в форму, а не в список.</summary>
        [RelayCommand]
        private Task AddPressure() => Shell.Current.GoToAsync(AppRoutes.PressureEdit);

        [RelayCommand]
        private Task OpenPressure() => Shell.Current.GoToAsync(AppRoutes.Pressure);

        /// <summary>
        /// Тап по карточке анализов открывает весь список — так же, как тап по карточке
        /// давления открывает дневник. Разное поведение у двух одинаковых на вид карточек
        /// человек воспринимает как поломку, а не как замысел.
        /// </summary>
        [RelayCommand]
        private Task OpenHistory() => Shell.Current.GoToAsync(AppRoutes.History);

        /// <summary>Сводка вне нормы ведёт туда, где эти показатели перечислены и отбираются.</summary>
        [RelayCommand]
        private Task OpenTrends() => Shell.Current.GoToAsync(AppRoutes.Trends);

        private async Task LoadAsync()
        {
            Title = S.Dash_Title;

            IReadOnlyList<ParameterTrend> trends = await _analysis.GetLatestTrendsAsync();
            BloodTest? latest = await _repository.GetLatestAsync();
            int count = await _repository.CountAsync();

            HasData = latest is not null;
            IsEmpty = latest is null;

            LastTestTitle = latest is null
                ? S.Dash_NoTests
                : string.Format(S.Dash_LastTest, latest.Title);

            HighCount = trends.Count(t => t.Status is ParameterStatus.High);
            LowCount = trends.Count(t => t.Status is ParameterStatus.Low);

            AllInRange = HasData && HighCount == 0 && LowCount == 0;

            // Показателей считаем столько, сколько их в сведённом дне: два бланка от одного
            // числа — это по-прежнему один день, и в счёт идут оба.
            Summary = latest is null
                ? S.Dash_EmptyHint
                : BuildSummary(trends.Count, count, HighCount + LowCount);

            await LoadPressureAsync();

            // Копия делается один раз за запуск и только если данные изменились.
            // Молча и в фоне: это обслуживание, а не действие пользователя, и мешать ему нечем.
            _ = BackupQuietlyAsync();
        }

        /// <summary>
        /// Автосохранение не должно ни задерживать экран, ни показывать ошибок: папка могла
        /// исчезнуть, разрешение — быть отозвано, и человек узнает об этом в настройках,
        /// где видно дату последней копии, а не всплывающим окном на главной.
        /// </summary>
        private async Task BackupQuietlyAsync()
        {
            if (_backupChecked)
            {
                return;
            }

            _backupChecked = true;

            try
            {
                await _backup.BackupIfChangedAsync();
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"[MedicalResultsTracker] Автосохранение не удалось: {exception}");
            }
        }

        private async Task LoadPressureAsync()
        {
            if (await _pressure.GetLatestAsync() is not BloodPressureReading latest)
            {
                PressureSummary = S.Bp_NoneYet;
                PressureColor = StatusPalette.Unknown;
                return;
            }

            PressureSummary = string.Format(
                S.Bp_LastReading,
                latest.Display,
                latest.MeasuredAt.ToString("g", CultureInfo.CurrentCulture));

            PressureColor = latest.IsAbove(BloodPressureTarget.Systolic, BloodPressureTarget.Diastolic)
                ? StatusPalette.High
                : latest.IsBelow(BloodPressureTarget.SystolicLow, BloodPressureTarget.DiastolicLow)
                    ? StatusPalette.Low
                    : StatusPalette.Normal;
        }

        private static string BuildSummary(int parameterCount, int totalTests, int attentionCount)
        {
            string tests = totalTests == 1 ? S.Dash_OneTest : string.Format(S.Dash_ManyTests, totalTests);

            string attention = attentionCount switch
            {
                0 => S.Dash_AllInRange,
                1 => S.Dash_OneOut,
                _ => string.Format(S.Dash_ManyOut, attentionCount)
            };

            return string.Format(S.Dash_Summary, parameterCount, attention, tests);
        }
    }
}
