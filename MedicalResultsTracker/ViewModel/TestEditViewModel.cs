using MedicalResultsTracker.Model;
using MedicalResultsTracker.Services.Ai;
using MedicalResultsTracker.Services.UI;
using MedicalResultsTracker.Services.Database;

namespace MedicalResultsTracker.ViewModel
{
    /// <summary>Ввод и правка одного анализа. Работает и как экран просмотра.</summary>
    public partial class TestEditViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IBloodTestRepository _repository;
        private readonly IAnalyteCatalog _catalog;
        private readonly IAiConsentService _consent;
        private readonly IAiAssistant _assistant;

        [ObservableProperty]
        private DateTime _date = DateTime.Today;

        [ObservableProperty]
        private string? _laboratory;

        [ObservableProperty]
        private string? _notes;

        [ObservableProperty]
        private bool _isExisting;

        [ObservableProperty]
        private Analyte? _selectedAnalyte;

        [ObservableProperty]
        private string _assistantHint = string.Empty;

        [ObservableProperty]
        private bool _hasAssistantHint;

        private Guid _testId = Guid.NewGuid();
        private DataOrigin _origin = DataOrigin.Manual;
        private string? _sourceFilePath;

        public TestEditViewModel(
            IBloodTestRepository repository,
            IAnalyteCatalog catalog,
            IAiConsentService consent,
            IAiAssistant assistant)
        {
            _repository = repository;
            _catalog = catalog;
            _consent = consent;
            _assistant = assistant;

            Title = "Новый анализ";
        }

        public ObservableCollection<ParameterRowViewModel> Rows { get; } = new();

        public ObservableCollection<Analyte> Catalog { get; } = new();

        partial void OnAssistantHintChanged(string value) => HasAssistantHint = !string.IsNullOrWhiteSpace(value);

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue(AppRoutes.TestIdParameter, out object? value) &&
                Guid.TryParse(Convert.ToString(value), out Guid id))
            {
                _testId = id;
                IsExisting = true;
                Title = "Анализ";
            }
            else
            {
                ResetToNew();
            }
        }

        public override Task InitializeAsync() => RunAsync(LoadAsync, "Не удалось открыть анализ");

        [RelayCommand]
        private void AddRow() => Rows.Add(new ParameterRowViewModel());

        [RelayCommand]
        private void AddFromCatalog()
        {
            if (SelectedAnalyte is null)
            {
                return;
            }

            Rows.Add(ParameterRowViewModel.FromAnalyte(SelectedAnalyte));
            SelectedAnalyte = null;
        }

        [RelayCommand]
        private void RemoveRow(ParameterRowViewModel? row)
        {
            if (row is not null)
            {
                Rows.Remove(row);
            }
        }

        /// <summary>
        /// Копирует набор показателей из прошлого анализа: обычно сдаётся одна и та же панель,
        /// и переписывать названия руками каждый раз бессмысленно.
        /// </summary>
        [RelayCommand]
        private Task CopyFromPrevious() => RunAsync(async () =>
        {
            BloodTest? previous = await _repository.GetPreviousAsync(Date);

            if (previous is null)
            {
                await Dialog.AlertAsync("Нет данных", "Более раннего анализа в истории нет.");
                return;
            }

            foreach (BloodParameter parameter in previous.Parameters)
            {
                ParameterRowViewModel row = new(parameter.Clone())
                {
                    // Названия и нормы переносим, значения — нет, их пользователь вводит заново.
                    ValueText = string.Empty,
                };

                Rows.Add(row);
            }
        }, "Не удалось скопировать показатели");

        [RelayCommand]
        private Task Save() => RunAsync(async () =>
        {
            List<ParameterRowViewModel> filled = Rows.Where(r => !r.IsEmpty).ToList();

            if (filled.Count == 0)
            {
                await Dialog.AlertAsync("Пусто", "Добавьте хотя бы один показатель.");
                return;
            }

            if (filled.Any(r => string.IsNullOrWhiteSpace(r.Name)))
            {
                await Dialog.AlertAsync("Не хватает названия", "У каждой заполненной строки должно быть название показателя.");
                return;
            }

            BloodTest test = new()
            {
                Id = _testId,
                Date = Date,
                Laboratory = string.IsNullOrWhiteSpace(Laboratory) ? null : Laboratory.Trim(),
                Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim(),
                SourceFilePath = _sourceFilePath,
                Origin = _origin,
            };

            test.Parameters = filled
                .Select((row, index) => row.ToModel(test.Id, index))
                .ToList();

            await _repository.SaveAsync(test);

            // Новые показатели, которых нет в справочнике, запоминаем — в следующий раз подставятся сами.
            await RememberNewAnalytesAsync(test);

            await Shell.Current.GoToAsync("..");
        }, "Не удалось сохранить анализ");

        [RelayCommand]
        private Task Delete() => RunAsync(async () =>
        {
            if (!IsExisting)
            {
                await Shell.Current.GoToAsync("..");
                return;
            }

            bool confirmed = await Dialog.ConfirmAsync(
                "Удалить анализ?",
                "Запись будет удалена с устройства безвозвратно.",
                "Удалить");

            if (!confirmed)
            {
                return;
            }

            await _repository.DeleteAsync(_testId);
            await Shell.Current.GoToAsync("..");
        }, "Не удалось удалить анализ");

        [RelayCommand]
        private Task Cancel() => Shell.Current.GoToAsync("..");

        /// <summary>
        /// Кнопка "распознать бланк". Пока провайдер не подключён, честно говорим об этом,
        /// вместо того чтобы делать вид, что функция есть.
        /// </summary>
        [RelayCommand]
        private Task ScanDocument() => RunAsync(async () =>
        {
            if (!_assistant.IsAvailable(AiConsentScope.DocumentRecognition))
            {
                await Dialog.AlertAsync(
                    "Распознавание недоступно",
                    _consent.IsAllowed(AiConsentScope.DocumentRecognition)
                        ? "Согласие дано, но ИИ-провайдер не подключён к сборке."
                        : "Функция требует явного согласия на отправку изображения. Включить её можно в настройках.");
                return;
            }

            FileResult? photo = await MediaPicker.Default.CapturePhotoAsync();

            if (photo is null)
            {
                return;
            }

            AiDraft? draft = await _assistant.ExtractAsync(photo.FullPath);

            if (draft is null)
            {
                await Dialog.AlertAsync("Не получилось", "Ассистент не смог разобрать бланк. Введите значения вручную.");
                return;
            }

            ApplyDraft(draft);
        }, "Не удалось распознать документ");

        /// <summary>Черновик от ассистента только заполняет поля — сохранение всегда за пользователем.</summary>
        private void ApplyDraft(AiDraft draft)
        {
            if (draft.Date is DateTime date)
            {
                Date = date;
            }

            if (!string.IsNullOrWhiteSpace(draft.Laboratory))
            {
                Laboratory = draft.Laboratory;
            }

            foreach (AiDraftRow row in draft.Rows)
            {
                Rows.Add(new ParameterRowViewModel
                {
                    Code = row.Code,
                    Name = row.Name,
                    Unit = row.Unit,
                    ValueText = row.Value?.ToString("0.####") ?? row.TextValue ?? string.Empty,
                    RefMinText = row.RefMin?.ToString("0.####") ?? string.Empty,
                    RefMaxText = row.RefMax?.ToString("0.####") ?? string.Empty,
                });
            }

            _origin = DataOrigin.AssistedReview;

            AssistantHint = draft.Warnings.Count > 0
                ? $"Проверьте распознанное. Замечания: {string.Join("; ", draft.Warnings)}"
                : "Проверьте распознанные значения перед сохранением.";
        }

        private async Task LoadAsync()
        {
            Catalog.Clear();

            foreach (Analyte analyte in await _catalog.GetAllAsync())
            {
                Catalog.Add(analyte);
            }

            if (!IsExisting)
            {
                return;
            }

            BloodTest? test = await _repository.GetAsync(_testId);

            if (test is null)
            {
                ResetToNew();
                return;
            }

            Date = test.Date;
            Laboratory = test.Laboratory;
            Notes = test.Notes;
            _origin = test.Origin;
            _sourceFilePath = test.SourceFilePath;

            Rows.Clear();

            foreach (BloodParameter parameter in test.Parameters)
            {
                Rows.Add(new ParameterRowViewModel(parameter));
            }
        }

        private async Task RememberNewAnalytesAsync(BloodTest test)
        {
            foreach (BloodParameter parameter in test.Parameters.Where(p => string.IsNullOrWhiteSpace(p.Code)))
            {
                string code = MakeCode(parameter.Name);

                if (await _catalog.FindAsync(code) is not null)
                {
                    continue;
                }

                await _catalog.SaveAsync(new Analyte
                {
                    Code = code,
                    Name = parameter.Name,
                    Unit = parameter.Unit,
                    Category = "Мои показатели",
                    RefMin = parameter.RefMin,
                    RefMax = parameter.RefMax,
                    IsBuiltIn = false,
                });
            }
        }

        private static string MakeCode(string name)
        {
            string cleaned = new(name.Trim().ToUpperInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray());

            return cleaned.Length > 32 ? cleaned[..32] : cleaned;
        }

        private void ResetToNew()
        {
            _testId = Guid.NewGuid();
            _origin = DataOrigin.Manual;
            _sourceFilePath = null;
            IsExisting = false;
            Title = "Новый анализ";
            Date = DateTime.Today;
            Laboratory = null;
            Notes = null;
            AssistantHint = string.Empty;
            Rows.Clear();
        }


    }
}
