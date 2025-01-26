using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Reporting.Interfaces;

namespace DatabaseBenchmark.Reporting
{
    public class ReportFormatterFactory : IAllowedValuesProvider
    {
        private readonly Dictionary<string, Func<IReportFormatter>> _factories =
            new()
            {
                ["Text"] = () => new TextTableReportFormatter(new ValueFormatter()),
                ["Csv"] = () => new CsvReportFormatter()
            };

        public IEnumerable<string> Options => _factories.Keys;

        public IReportFormatter Create(string type)
        {
            if (_factories.TryGetValue(type, out var factory))
            {
                return factory();
            }

            throw new InputArgumentException($"Unknown formatter type \"{type}\"");
        }
    }
}
