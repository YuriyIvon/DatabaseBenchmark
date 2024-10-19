using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.Reporting.Interfaces
{
    public interface IReportFormatter
    {
        void Print(Stream stream, LightweightDataTable results);
    }
}
