using MedicalResultsTracker.Model;
using MedicalResultsTracker.Resources.Strings;
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
            ITextImportService import)
        {
            _repository = repository;
            _catalog = catalog;
            _import = import;

            Title = S.Edit_TitleNew;
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
                Title = S.Edit_TitleExisting;
            }
            else
            {
                ResetToNew();
            }
        }

        public override Task InitializeAsync() => RunAsync(LoadAsync, S.Err_OpenTest);

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
                await Dialog.AlertAsync(S.Trend_ChartNoData, S.Edit_NoPrevBody);
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
        }, S.Err_CopyRows);

        [RelayCommand]
        private Task Save() => RunAsync(async () =>
        {
            List<ParameterRowViewModel> filled = Rows.Where(r => !r.IsEmpty).ToList();

            if (filled.Count == 0)
            {
                await Dialog.AlertAsync(S.Edit_EmptyTitle, S.Edit_EmptyBody);
                return;
            }

            if (filled.Any(r => string.IsNullOrWhiteSpace(r.Name)))
            {
                await Dialog.AlertAsync(S.CatEdit_NoNameTitle, S.Edit_NoNameBody);
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
        }, S.Err_SaveTest);

        [RelayCommand]
        private Task Delete() => RunAsync(async () =>
        {
            if (!IsExisting)
            {
                await Shell.Current.GoToAsync("..");
                return;
            }

            bool confirmed = await Dialog.ConfirmAsync(
                S.Edit_DeleteTitle,
                S.Edit_DeleteBody,
                S.Common_Delete);

            if (!confirmed)
            {
                return;
            }

            await _repository.DeleteAsync(_testId);
            await Shell.Current.GoToAsync("..");
        }, S.Err_DeleteTest);

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
                S.Edit_PromptCopiedTitle,
                S.Edit_PromptCopiedBody);
        }, S.Err_Prompt);

        /// <summary>Разбирает текст из буфера в строки показателей.</summary>
        [RelayCommand]
        private Task PasteRows() => RunAsync(async () =>
        {
            string? text = await Clipboard.Default.GetTextAsync();

            if (string.IsNullOrWhiteSpace(text))
            {
                await Dialog.AlertAsync(S.Edit_ClipEmptyTitle, S.Edit_ClipEmptyBody);
                return;
            }

            AiDraft draft = _import.Parse(text);

            if (draft.Rows.Count == 0)
            {
                await Dialog.AlertAsync(
                    S.Edit_NoRowsTitle,
                    S.Edit_NoRowsBody);
                return;
            }

            ApplyDraft(draft);
        }, S.Err_Parse);

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

            string added = string.Format(S.Edit_Added, draft.Rows.Count);

            AssistantHint = draft.Warnings.Count > 0
                ? string.Format(S.Edit_AddedWarn, added, string.Join("; ", draft.Warnings))
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
                    Category = S.Edit_MyValues,
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
            Title = S.Edit_TitleNew;
            Date = DateTime.Today;
            Laboratory = null;
            Notes = null;
            AssistantHint = string.Empty;
            Rows.Clear();
        }


    }
}
