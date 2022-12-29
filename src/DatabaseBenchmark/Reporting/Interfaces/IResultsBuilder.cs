using DatabaseBenchmark.Reporting.Model;
using System.Data;

namespace DatabaseBenchmark.Reporting.Interfaces
{
    public interface IResultsBuilder
    {
        DataTable Build(IEnumerable<MetricsCollection> metrics);
    }
}
