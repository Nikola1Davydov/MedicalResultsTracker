using System.Globalization;
using MedicalResultsTracker.Resources.Strings;
using System.Text;
using System.Text.Json.Serialization;
using MedicalResultsTracker.Model;
using MedicalResultsTracker.Services.Analysis;
using MedicalResultsTracker.Services.Database;

namespace MedicalResultsTracker.Services.Export
{
    /// <summary>
    /// Выгрузка истории в файл. Файл кладётся в кэш приложения, дальше пользователь сам решает,
    /// что с ним делать через системный диалог "Поделиться" — приложение никуда ничего не отправляет.
    /// </summary>
    public sealed class ExportService : IExportService
    {
        // Точка с запятой + числа в текущей культуре: так файл открывается в Excel без "мастера импорта".
        private const char Separator = ';';

        private static readonly JsonSerializerOptions BackupJsonOptions = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        private readonly IBloodTestRepository _repository;
        private readonly IAnalysisService _analysis;

        public ExportService(IBloodTestRepository repository, IAnalysisService analysis)
        {
            _repository = repository;
            _analysis = analysis;
        }

        public async Task<string> ExportMatrixCsvAsync()
        {
            IReadOnlyList<BloodTest> tests = await _repository.GetAllAsync().ConfigureAwait(false);

            // Слева направо — от старых к новым, чтобы динамика читалась естественно.
            List<BloodTest> ordered = tests.OrderBy(t => t.Date).ToList();

            List<BloodParameter> allParameters = ordered.SelectMany(t => t.Parameters).ToList();

            List<IGrouping<string, BloodParameter>> rows = allParameters
                .GroupBy(_analysis.GetKey)
                .OrderBy(g => g.Last().Name)
                .ToList();

            StringBuilder builder = new();

            builder.Append(Join(S.Csv_Parameter, S.Csv_Unit, S.Csv_Reference));

            foreach (BloodTest test in ordered)
            {
                builder.Append(Separator).Append(Escape(test.Date.ToString("dd.MM.yyyy")));
            }

            builder.AppendLine();

            foreach (IGrouping<string, BloodParameter> row in rows)
            {
                BloodParameter newest = row.Last();

                builder.Append(Join(newest.Name, newest.Unit ?? string.Empty, newest.Range.ToString()));

                foreach (BloodTest test in ordered)
                {
                    BloodParameter? measurement = test.Parameters.FirstOrDefault(p => _analysis.GetKey(p) == row.Key);

                    builder.Append(Separator).Append(Escape(FormatValue(measurement)));
                }

                builder.AppendLine();
            }

            return await WriteAsync($"medical-results-{DateTime.Now:yyyy-MM-dd}.csv", builder.ToString())
                .ConfigureAwait(false);
        }

        public async Task<string> ExportFlatCsvAsync()
        {
            IReadOnlyList<BloodTest> tests = await _repository.GetAllAsync().ConfigureAwait(false);

            StringBuilder builder = new();

            builder.AppendLine(Join(
                S.Csv_Date, S.Csv_Lab, S.Csv_Code, S.Csv_Parameter, S.Csv_Value, S.Csv_Unit, S.Csv_Min, S.Csv_Max, S.Csv_Status, S.Csv_Comment));

            foreach (BloodTest test in tests.OrderBy(t => t.Date))
            {
                foreach (BloodParameter parameter in test.Parameters)
                {
                    builder.AppendLine(Join(
                        test.Date.ToString("dd.MM.yyyy"),
                        test.Laboratory ?? string.Empty,
                        parameter.Code ?? string.Empty,
                        parameter.Name,
                        FormatValue(parameter),
                        parameter.Unit ?? string.Empty,
                        FormatNumber(parameter.RefMin),
                        FormatNumber(parameter.RefMax),
                        DescribeStatus(parameter.Status),
                        parameter.Comment ?? string.Empty));
                }
            }

            return await WriteAsync($"medical-results-flat-{DateTime.Now:yyyy-MM-dd}.csv", builder.ToString())
                .ConfigureAwait(false);
        }

        public async Task<string> ExportBackupAsync()
        {
            IReadOnlyList<BloodTest> tests = await _repository.GetAllAsync().ConfigureAwait(false);

            string json = JsonSerializer.Serialize(new BackupFile { Tests = tests.ToList() }, BackupJsonOptions);

            return await WriteAsync($"medical-results-backup-{DateTime.Now:yyyy-MM-dd}.json", json)
                .ConfigureAwait(false);
        }

        public async Task<int> ImportBackupAsync(string filePath, bool replaceExisting = false)
        {
            string json = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);

            BackupFile? backup = JsonSerializer.Deserialize<BackupFile>(json, BackupJsonOptions);

            if (backup?.Tests is not { Count: > 0 })
            {
                return 0;
            }

            IReadOnlyList<BloodTest> existing = await _repository.GetAllAsync().ConfigureAwait(false);
            HashSet<Guid> existingIds = existing.Select(t => t.Id).ToHashSet();

            int imported = 0;

            foreach (BloodTest test in backup.Tests)
            {
                if (existingIds.Contains(test.Id) && !replaceExisting)
                {
                    continue;
                }

                foreach (BloodParameter parameter in test.Parameters)
                {
                    parameter.TestId = test.Id;

                    // Новые идентификаторы строк: в базе может уже лежать строка с таким Id
                    // от частично импортированной ранее копии, и вставка упала бы на первичном ключе.
                    parameter.Id = Guid.NewGuid();
                }

                await _repository.SaveAsync(test).ConfigureAwait(false);
                imported++;
            }

            return imported;
        }

        public async Task<string> BuildTextSummaryAsync(int maxTests = 6)
        {
            IReadOnlyList<BloodTest> all = await _repository.GetAllAsync().ConfigureAwait(false);

            // GetAllAsync отдаёт свежие сверху: берём последние N и разворачиваем в хронологию.
            List<BloodTest> ordered = (maxTests > 0 ? all.Take(maxTests) : all)
                .OrderBy(t => t.Date)
                .ToList();

            if (ordered.Count == 0)
            {
                return S.Txt_Empty;
            }

            StringBuilder builder = new();

            builder.AppendLine(S.Txt_Header);
            builder.AppendLine(S.Txt_NoPersonal);
            builder.AppendLine(S.Txt_RefNote);
            builder.AppendLine();

            BloodTest latest = ordered[^1];

            builder.AppendLine(string.Format(S.Txt_Latest, latest.Title));
            builder.AppendLine();

            List<IGrouping<string, BloodParameter>> rows = ordered
                .SelectMany(t => t.Parameters)
                .GroupBy(_analysis.GetKey)
                .OrderBy(g => g.Last().Name)
                .ToList();

            builder.Append($"| {S.Csv_Parameter} | {S.Csv_Unit} | {S.Csv_Reference} |");

            foreach (BloodTest test in ordered)
            {
                builder.Append($" {test.Date:dd.MM.yyyy} |");
            }

            builder.AppendLine();
            builder.Append("|---|---|---|");
            builder.Append(string.Concat(Enumerable.Repeat("---|", ordered.Count)));
            builder.AppendLine();

            foreach (IGrouping<string, BloodParameter> row in rows)
            {
                BloodParameter newest = row.Last();
                string range = newest.Range.IsDefined ? newest.Range.ToString() : S.Common_None;

                builder.Append($"| {Cell(newest.Name)} | {Cell(newest.Unit) ?? S.Common_None} | {range} |");

                foreach (BloodTest test in ordered)
                {
                    BloodParameter? measurement = test.Parameters.FirstOrDefault(p => _analysis.GetKey(p) == row.Key);

                    builder.Append($" {(measurement is null ? S.Common_None : FormatValue(measurement))} |");
                }

                builder.AppendLine();
            }

            List<BloodParameter> outOfRange = latest.Parameters
                .Where(p => p.Status is ParameterStatus.Low or ParameterStatus.High)
                .ToList();

            builder.AppendLine();

            builder.AppendLine(outOfRange.Count == 0
                ? S.Txt_AllInRange
                : string.Format(S.Txt_OutOfRange, string.Join(", ", outOfRange.Select(p =>
                    $"{p.Name} {FormatValue(p)} {p.Unit} ({p.Range})".Replace("  ", " ").Trim()))));

            return builder.ToString();
        }

        public async Task ShareTextAsync(string text, string title)
        {
            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Title = title,
                Subject = title,
                Text = text,
            }).ConfigureAwait(false);
        }

        public async Task CopyToClipboardAsync(string text) =>
            await Clipboard.Default.SetTextAsync(text).ConfigureAwait(false);

        public async Task ShareAsync(string filePath, string title)
        {
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = title,
                File = new ShareFile(filePath),
            }).ConfigureAwait(false);
        }

        private static async Task<string> WriteAsync(string fileName, string content)
        {
            string path = Path.Combine(FileSystem.CacheDirectory, fileName);

            // BOM — иначе Excel на Windows ломает кириллицу.
            await File.WriteAllTextAsync(path, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true))
                .ConfigureAwait(false);

            return path;
        }

        /// <summary>Вертикальная черта в названии разорвала бы markdown-таблицу.</summary>
        private static string? Cell(string? value) => value?.Replace("|", "\\|");

        private static string FormatValue(BloodParameter? parameter) => parameter switch
        {
            null => string.Empty,
            { Value: double value } => value.ToString("0.####", CultureInfo.CurrentCulture),
            _ => parameter.TextValue ?? string.Empty
        };

        private static string FormatNumber(double? value) =>
            value?.ToString("0.####", CultureInfo.CurrentCulture) ?? string.Empty;

        private static string DescribeStatus(ParameterStatus status) => status switch
        {
            ParameterStatus.Low => S.Csv_StatusLow,
            ParameterStatus.High => S.Csv_StatusHigh,
            ParameterStatus.Normal => S.Csv_StatusNormal,
            _ => string.Empty
        };

        private static string Join(params string[] values) => string.Join(Separator, values.Select(Escape));

        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            bool needsQuotes = value.Contains(Separator) || value.Contains('"') || value.Contains('\n') || value.Contains('\r');

            return needsQuotes ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
        }

        private sealed class BackupFile
        {
            public int Version { get; set; } = 1;

            public DateTime ExportedUtc { get; set; } = DateTime.UtcNow;

            public List<BloodTest> Tests { get; set; } = new();
        }
    }
}
