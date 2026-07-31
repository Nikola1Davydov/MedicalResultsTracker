using MedicalResultsTracker.Model;
using MedicalResultsTracker.Services.Ai;
using MedicalResultsTracker.Services.Database;
using MedicalResultsTracker.Services.Import;
using MedicalResultsTracker.Services.UI;

namespace MedicalResultsTracker.ViewModel
{
    /// <summary>Ввод и правка одного анализа. Работает и как экран просмотра.</summary>
    public partial class TestEditViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IBloodTestRepository _repository;
        private readonly IAnalyteCatalog _catalog;
        private readonly IAiConsentService _consent;
        private readonly IAiAssistant _assistant;
        private readonly ITextImportService _import;

        [ObservableProperty]
        private DateTime _date = DateTime.Today;

        [ObservableProperty]
        private string? _laboratory;

        [ObservableProperty]
        private string? _notes;

        [ObservableProperty]
        private bool _isExisting;

        [ObservableProperty]
        private string _analyteQuery = string.Empty;

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
            IAiAssistant assistant,
            ITextImportService import)
        {
            _repository = repository;
            _catalog = catalog;
            _consent = consent;
            _assistant = assistant;
            _import = import;

            Title = "Новый анализ";
        }

        public ObservableCollection<ParameterRowViewModel> Rows { get; } = new();

        /// <summary>Подсказки из справочника под строкой поиска. Держим список коротким.</summary>
        public ObservableCollection<Analyte> Suggestions { get; } = new();

        partial void OnAssistantHintChanged(string value) => HasAssistantHint = !string.IsNullOrWhiteSpace(value);

        partial void OnAnalyteQueryChanged(string value) => _ = RefreshSuggestionsAsync(value);

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

        /// <summary>Добавляет показатель из подсказок и сбрасывает поиск — обычно следом ищут следующий.</summary>
        [RelayCommand]
        private void AddAnalyte(Analyte? analyte)
        {
            if (analyte is null)
            {
                return;
            }

            Rows.Add(ParameterRowViewModel.FromAnalyte(analyte));
            AnalyteQuery = string.Empty;
        }

        /// <summary>Заводит строку с тем названием, что набрано в поиске: расширенной панели в справочнике может и не быть.</summary>
        [RelayCommand]
        private void AddTypedName()
        {
            string name = AnalyteQuery.Trim();

            Rows.Add(name.Length == 0
                ? new ParameterRowViewModel()
                : new ParameterRowViewModel { Name = name });

            AnalyteQuery = string.Empty;
        }

        private async Task RefreshSuggestionsAsync(string query)
        {
            try
            {
                IReadOnlyList<Analyte> found = await _catalog.SearchAsync(query, limit: 8);

                Suggestions.Clear();

                foreach (Analyte analyte in found)
                {
                    Suggestions.Add(analyte);
                }
            }
            catch (Exception exception)
            {
                // Подсказки — вспомогательная вещь: молча остаёмся без них, ввод продолжает работать.
                Debug.WriteLine($"[MedicalResultsTracker] Не удалось получить подсказки: {exception}");
            }
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

            // Код проставляется до сохранения: именно по нему измерение найдёт своих предшественников.
            // Без этого строка, набранная руками, и та же строка, выбранная из справочника,
            // разъезжаются на два независимых графика.
            foreach (BloodParameter parameter in test.Parameters)
            {
                parameter.Code = await ResolveCodeAsync(parameter);
            }

            await _repository.SaveAsync(test);

            // Показатели, которых нет в справочнике, запоминаем — в следующий раз подставятся сами.
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
        /// Кладёт в буфер запрос для чат-бота. Дальше пользователь сам фотографирует бланк
        /// в своём чате, получает текст в нужном формате и возвращается сюда с кнопкой «Вставить».
        /// Распознаванием занимается чат пользователя — приложению не нужны ни ключи, ни сеть.
        /// </summary>
        [RelayCommand]
        private Task CopyImportPrompt() => RunAsync(async () =>
        {
            await Clipboard.Default.SetTextAsync(_import.PromptForChat);

            await Dialog.AlertAsync(
                "Запрос скопирован",
                "Вставьте его в любой чат-бот вместе с фотографией бланка. Полученный ответ " +
                "скопируйте и вернитесь сюда — кнопка «Вставить из буфера» разложит его по строкам.");
        }, "Не удалось скопировать запрос");

        /// <summary>Разбирает текст из буфера в строки показателей.</summary>
        [RelayCommand]
        private Task PasteRows() => RunAsync(async () =>
        {
            string? text = await Clipboard.Default.GetTextAsync();

            if (string.IsNullOrWhiteSpace(text))
            {
                await Dialog.AlertAsync("Буфер пуст", "Сначала скопируйте таблицу с результатами.");
                return;
            }

            AiDraft draft = _import.Parse(text);

            if (draft.Rows.Count == 0)
            {
                await Dialog.AlertAsync(
                    "Не разобрано",
                    "В тексте не нашлось строк показателей. Каждая строка должна выглядеть так:\n\n" +
                    "Ферритин | 18 | мкг/л | 30 | 300");
                return;
            }

            ApplyDraft(draft);
        }, "Не удалось разобрать текст");

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

            string added = $"Добавлено строк: {draft.Rows.Count}. Проверьте значения перед сохранением.";

            AssistantHint = draft.Warnings.Count > 0
                ? $"{added} Не разобрано: {string.Join("; ", draft.Warnings)}"
                : added;
        }

        private async Task LoadAsync()
        {
            await RefreshSuggestionsAsync(AnalyteQuery);

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

        /// <summary>
        /// Подбирает код показателя: уже проставленный, затем совпадение по названию в каталоге,
        /// и только потом новый код из названия.
        /// </summary>
        private async Task<string> ResolveCodeAsync(BloodParameter parameter)
        {
            if (!string.IsNullOrWhiteSpace(parameter.Code))
            {
                return parameter.Code.Trim().ToUpperInvariant();
            }

            Analyte? known = await _catalog.FindByNameAsync(parameter.Name);

            return known?.Code ?? AnalyteCode.FromName(parameter.Name);
        }

        private async Task RememberNewAnalytesAsync(BloodTest test)
        {
            foreach (BloodParameter parameter in test.Parameters)
            {
                if (string.IsNullOrWhiteSpace(parameter.Code) || await _catalog.FindAsync(parameter.Code) is not null)
                {
                    continue;
                }

                await _catalog.SaveAsync(new Analyte
                {
                    Code = parameter.Code,
                    Name = parameter.Name,
                    Unit = parameter.Unit,
                    Category = "Мои показатели",
                    RefMin = parameter.RefMin,
                    RefMax = parameter.RefMax,
                    IsBuiltIn = false,
                });
            }
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
