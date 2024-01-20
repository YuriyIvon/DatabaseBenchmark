using System.Data;

namespace DatabaseBenchmark.Reporting.Interfaces
{
    public interface IReportFormatter
    {
        void Print(Stream stream, DataTable results);
    }
}
