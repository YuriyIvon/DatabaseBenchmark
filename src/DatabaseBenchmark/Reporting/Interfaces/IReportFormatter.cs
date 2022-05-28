using System.Data;

namespace DatabaseBenchmark.Interfaces.Reporting
{
    public interface IReportFormatter
    {
        void Print(Stream stream, DataTable results);
    }
}
