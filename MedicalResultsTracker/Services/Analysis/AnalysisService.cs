using MedicalResultsTracker.Model;
using MedicalResultsTracker.Services.Database;

namespace MedicalResultsTracker.Services.Analysis
{
    /// <inheritdoc cref="IAnalysisService"/>
    public sealed class AnalysisService : IAnalysisService
    {
        private readonly IBloodTestRepository _repository;

        public AnalysisService(IBloodTestRepository repository)
        {
            _repository = repository;
        }

        public string GetKey(BloodParameter parameter) =>
            string.IsNullOrWhiteSpace(parameter.Code)
                ? parameter.Name.Trim().ToLowerInvariant()
                : parameter.Code.Trim().ToUpperInvariant();

        public async Task<IReadOnlyList<ParameterTrend>> GetLatestTrendsAsync()
        {
            IReadOnlyList<BloodTest> tests = await _repository.GetAllAsync().ConfigureAwait(false);

            if (tests.Count == 0)
            {
                return Array.Empty<ParameterTrend>();
            }

            // GetAllAsync отдаёт свежие сверху.
            BloodTest current = tests[0];
            BloodTest? previous = tests.Count > 1 ? tests[1] : null;

            return BuildTrends(current, previous);
        }

        public async Task<IReadOnlyList<ParameterTrend>> CompareAsync(Guid currentTestId, Guid? previousTestId = null)
        {
            IReadOnlyList<BloodTest> tests = await _repository.GetAllAsync().ConfigureAwait(false);

            BloodTest? current = tests.FirstOrDefault(t => t.Id == currentTestId);

            if (current is null)
            {
                return Array.Empty<ParameterTrend>();
            }

            BloodTest? previous = previousTestId is Guid id
                ? tests.FirstOrDefault(t => t.Id == id)
                : tests.Where(t => t.Date < current.Date).OrderByDescending(t => t.Date).FirstOrDefault();

            return BuildTrends(current, previous);
        }

        public async Task<IReadOnlyList<ParameterSeries>> GetSeriesAsync()
        {
            IReadOnlyList<BloodTest> tests = await _repository.GetAllAsync().ConfigureAwait(false);

            List<(BloodTest Test, BloodParameter Parameter)> measurements = tests
                .SelectMany(test => test.Parameters.Select(parameter => (Test: test, Parameter: parameter)))
                .Where(x => x.Parameter.Value is not null)
                .ToList();

            return measurements
                .GroupBy(x => GetKey(x.Parameter))
                .Select(group =>
                {
                    List<(BloodTest Test, BloodParameter Parameter)> ordered = group
                        .OrderBy(x => x.Test.Date)
                        .ToList();

                    (BloodTest Test, BloodParameter Parameter) newest = ordered[^1];

                    return new ParameterSeries
                    {
                        Key = group.Key,
                        Name = newest.Parameter.Name,
                        Unit = newest.Parameter.Unit,
                        Points = ordered.Select(x => new SeriesPoint
                        {
                            Date = x.Test.Date,
                            Value = x.Parameter.Value!.Value,
                            Status = x.Parameter.Status,
                            RefMin = x.Parameter.RefMin,
                            RefMax = x.Parameter.RefMax,
                        }).ToList(),
                    };
                })
                .OrderByDescending(s => s.Points.Count)
                .ThenBy(s => s.Name)
                .ToList();
        }

        public async Task<ParameterSeries?> GetSeriesAsync(string key)
        {
            IReadOnlyList<ParameterSeries> all = await GetSeriesAsync().ConfigureAwait(false);

            return all.FirstOrDefault(s => string.Equals(s.Key, key, StringComparison.OrdinalIgnoreCase));
        }

        private IReadOnlyList<ParameterTrend> BuildTrends(BloodTest current, BloodTest? previous)
        {
            Dictionary<string, BloodParameter> previousByKey = previous is null
                ? new Dictionary<string, BloodParameter>()
                : previous.Parameters
                    .GroupBy(GetKey)
                    .ToDictionary(g => g.Key, g => g.First());

            return current.Parameters
                .Select(parameter =>
                {
                    previousByKey.TryGetValue(GetKey(parameter), out BloodParameter? before);

                    return new ParameterTrend
                    {
                        Current = parameter,
                        Previous = before,
                        CurrentDate = current.Date,
                        PreviousDate = before is null ? null : previous?.Date,
                    };
                })
                .ToList();
        }
    }
}
