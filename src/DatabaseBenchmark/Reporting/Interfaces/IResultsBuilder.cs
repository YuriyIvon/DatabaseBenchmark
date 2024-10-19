using DatabaseBenchmark.Common;
using DatabaseBenchmark.Reporting.Model;

namespace DatabaseBenchmark.Reporting.Interfaces
{
    public interface IResultsBuilder
    {
        LightweightDataTable Build(IEnumerable<MetricsCollection> metrics);
    }
}
