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
        /// <summary>Разделитель берётся из культуры: Excel ждёт тот же, что и система.</summary>
        private static char Separator => CultureInfo.CurrentCulture.TextInfo.ListSeparator.FirstOrDefault(';');

        /// <summary>Сколько последних измерений давления класть в текст для чата.</summary>
        private const int MaxPressureRows = 30;

        private static readonly JsonSerializerOptions BackupJsonOptions = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        private readonly IBloodTestRepository _repository;
        private readonly IBloodPressureRepository _pressure;
        private readonly IAnalysisService _analysis;

        public ExportService(
            IBloodTestRepository repository,
            IBloodPressureRepository pressure,
            IAnalysisService analysis)
        {
            _repository = repository;
            _pressure = pressure;
            _analysis = analysis;
        }

        public async Task<string> ExportMatrixCsvAsync()
        {
            // Слева направо — от старых к новым, чтобы динамика читалась естественно.
            // Столбец — дата, а не бланк: тот же вид, что и в таблице на экране.
            ResultMatrix matrix = await _analysis.BuildMatrixAsync().ConfigureAwait(false);

            StringBuilder builder = new();

            builder.Append(Join(S.Csv_Parameter, S.Csv_Unit, S.Csv_Reference));

            foreach (DateTime date in matrix.Dates)
            {
                builder.Append(Separator).Append(Escape(date.ToString("d", CultureInfo.CurrentCulture)));
            }

            builder.AppendLine();

            foreach (MatrixLine line in matrix.Lines)
            {
                builder.Append(Join(line.Newest.Name, line.Newest.Unit ?? string.Empty, line.Newest.Range.ToString()));

                foreach (BloodParameter? measurement in line.Cells)
                {
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
                        test.Date.ToString("d", CultureInfo.CurrentCulture),
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

        public async Task<string> ExportPressureCsvAsync()
        {
            IReadOnlyList<BloodPressureReading> readings = await _pressure.GetAllAsync().ConfigureAwait(false);

            StringBuilder builder = new();

            builder.AppendLine(Join(
                S.Csv_Date, S.Csv_Time, S.Bp_Systolic, S.Bp_Diastolic, S.Bp_Pulse, S.Csv_Comment));

            // Старые сверху: дневник читают как хронологию.
            foreach (BloodPressureReading reading in readings.OrderBy(r => r.MeasuredAt))
            {
                builder.AppendLine(Join(
                    reading.MeasuredAt.ToString("d", CultureInfo.CurrentCulture),
                    reading.MeasuredAt.ToString("t", CultureInfo.CurrentCulture),
                    reading.Systolic.ToString(CultureInfo.CurrentCulture),
                    reading.Diastolic.ToString(CultureInfo.CurrentCulture),
                    reading.Pulse?.ToString(CultureInfo.CurrentCulture) ?? string.Empty,
                    reading.Note ?? string.Empty));
            }

            return await WriteAsync($"blood-pressure-{DateTime.Now:yyyy-MM-dd}.csv", builder.ToString())
                .ConfigureAwait(false);
        }

        public async Task<string> ExportBackupAsync()
        {
            IReadOnlyList<BloodTest> tests = await _repository.GetAllAsync().ConfigureAwait(false);
            IReadOnlyList<BloodPressureReading> pressure = await _pressure.GetAllAsync().ConfigureAwait(false);

            BackupFile backup = new()
            {
                Tests = tests.ToList(),
                Pressure = pressure.ToList(),
            };

            string json = JsonSerializer.Serialize(backup, BackupJsonOptions);

            return await WriteAsync($"medical-results-backup-{DateTime.Now:yyyy-MM-dd}.json", json)
                .ConfigureAwait(false);
        }

        public async Task<int> ImportBackupAsync(string filePath, bool replaceExisting = false)
        {
            string json = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);

            BackupFile? backup = JsonSerializer.Deserialize<BackupFile>(json, BackupJsonOptions);

            if (backup is null)
            {
                return 0;
            }

            int imported = 0;

            imported += await ImportTestsAsync(backup.Tests, replaceExisting).ConfigureAwait(false);
            imported += await ImportPressureAsync(backup.Pressure, replaceExisting).ConfigureAwait(false);

            return imported;
        }

        private async Task<int> ImportTestsAsync(List<BloodTest> tests, bool replaceExisting)
        {
            if (tests.Count == 0)
            {
                return 0;
            }

            IReadOnlyList<BloodTest> existing = await _repository.GetAllAsync().ConfigureAwait(false);
            HashSet<Guid> existingIds = existing.Select(t => t.Id).ToHashSet();

            int imported = 0;

            foreach (BloodTest test in tests)
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

        /// <summary>
        /// Копии, снятые до появления дневника давления, этого раздела не содержат —
        /// список приходит пустым, и восстановление такой копии проходит как раньше.
        /// </summary>
        private async Task<int> ImportPressureAsync(List<BloodPressureReading> readings, bool replaceExisting)
        {
            if (readings.Count == 0)
            {
                return 0;
            }

            IReadOnlyList<BloodPressureReading> existing = await _pressure.GetAllAsync().ConfigureAwait(false);
            HashSet<Guid> existingIds = existing.Select(r => r.Id).ToHashSet();

            int imported = 0;

            foreach (BloodPressureReading reading in readings)
            {
                if (existingIds.Contains(reading.Id) && !replaceExisting)
                {
                    continue;
                }

                await _pressure.SaveAsync(reading).ConfigureAwait(false);
                imported++;
            }

            return imported;
        }

        public async Task<string> BuildTextSummaryAsync(int maxTests = 6, IReadOnlyCollection<string>? onlyKeys = null)
        {
            // Ограничение считается в датах, а не в бланках: для читателя таблицы столбец — это день.
            ResultMatrix matrix = (await _analysis.BuildMatrixAsync().ConfigureAwait(false))
                .TakeLastDates(maxTests);

            if (matrix.Dates.Count == 0)
            {
                return S.Txt_Empty;
            }

            bool selection = onlyKeys is { Count: > 0 };

            if (selection)
            {
                HashSet<string> wanted = new(onlyKeys!, StringComparer.Ordinal);

                matrix = new ResultMatrix
                {
                    Dates = matrix.Dates,
                    Lines = matrix.Lines.Where(line => wanted.Contains(line.Key)).ToList(),
                };

                if (matrix.Lines.Count == 0)
                {
                    return S.Txt_Empty;
                }
            }

            BloodTest? latest = await _repository.GetLatestAsync().ConfigureAwait(false);

            StringBuilder builder = new();

            builder.AppendLine(S.Txt_Header);
            builder.AppendLine(S.Txt_NoPersonal);
            builder.AppendLine(S.Txt_RefNote);

            if (selection)
            {
                builder.AppendLine(S.Txt_Selection);
            }

            builder.AppendLine();

            builder.AppendLine(string.Format(
                S.Txt_Latest,
                latest?.Title ?? matrix.Dates[^1].ToString("d", CultureInfo.CurrentCulture)));

            builder.AppendLine();

            builder.Append($"| {S.Csv_Parameter} | {S.Csv_Unit} | {S.Csv_Reference} |");

            foreach (DateTime date in matrix.Dates)
            {
                builder.Append($" {date.ToString("d", CultureInfo.CurrentCulture)} |");
            }

            builder.AppendLine();
            builder.Append("|---|---|---|");
            builder.Append(string.Concat(Enumerable.Repeat("---|", matrix.Dates.Count)));
            builder.AppendLine();

            foreach (MatrixLine line in matrix.Lines)
            {
                string range = line.Newest.Range.IsDefined ? line.Newest.Range.ToString() : S.Common_None;

                builder.Append($"| {Cell(line.Newest.Name)} | {Cell(line.Newest.Unit) ?? S.Common_None} | {range} |");

                foreach (BloodParameter? measurement in line.Cells)
                {
                    builder.Append($" {(measurement is null ? S.Common_None : FormatValue(measurement))} |");
                }

                builder.AppendLine();
            }

            // Давление прикладывается только к полной выгрузке: когда человек отобрал
            // фильтрами три показателя, дневник давления рядом с ними — чужая тема.
            if (!selection)
            {
                await AppendPressureAsync(builder).ConfigureAwait(false);
            }

            // Итог по последнему столбцу, а не по последнему бланку: за один день их могло быть два.

            List<BloodParameter> outOfRange = matrix.Lines
                .Select(line => line.Cells[^1])
                .OfType<BloodParameter>()
                .Where(p => p.Status is ParameterStatus.Low or ParameterStatus.High)
                .ToList();

            builder.AppendLine();

            builder.AppendLine(outOfRange.Count == 0
                ? S.Txt_AllInRange
                : string.Format(S.Txt_OutOfRange, string.Join(", ", outOfRange.Select(p =>
                    $"{p.Name} {FormatValue(p)} {p.Unit} ({p.Range})".Replace("  ", " ").Trim()))));

            return builder.ToString();
        }

        /// <summary>
        /// Давление отдельным разделом, а не строками общей таблицы: там столбец на дату,
        /// а измерений за день бывает несколько, и время в них значимо.
        /// </summary>
        private async Task AppendPressureAsync(StringBuilder builder)
        {
            IReadOnlyList<BloodPressureReading> readings = await _pressure.GetAllAsync().ConfigureAwait(false);

            if (readings.Count == 0)
            {
                return;
            }

            builder.AppendLine();
            builder.AppendLine(S.Txt_Pressure);
            builder.AppendLine();
            builder.AppendLine($"| {S.Csv_Date} | {S.Csv_Time} | {S.Bp_Systolic} | {S.Bp_Diastolic} | {S.Bp_Pulse} |");
            builder.AppendLine("|---|---|---|---|---|");

            // Последние измерения, от старых к новым: столько влезает в чат, не утомляя.
            foreach (BloodPressureReading reading in readings
                .Take(MaxPressureRows)
                .OrderBy(r => r.MeasuredAt))
            {
                builder.AppendLine(
                    $"| {reading.MeasuredAt.ToString("d", CultureInfo.CurrentCulture)} " +
                    $"| {reading.MeasuredAt.ToString("t", CultureInfo.CurrentCulture)} " +
                    $"| {reading.Systolic} | {reading.Diastolic} " +
                    $"| {reading.Pulse?.ToString(CultureInfo.CurrentCulture) ?? S.Common_None} |");
            }
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

            /// <summary>
            /// Дневник давления. У копий, снятых до появления этого раздела, поля нет —
            /// десериализатор оставит пустой список, и восстановление такой копии не сломается.
            /// </summary>
            public List<BloodPressureReading> Pressure { get; set; } = new();
        }
    }
}
