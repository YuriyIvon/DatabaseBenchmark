using CsvHelper;
using DatabaseBenchmark.Interfaces.Reporting;
using System.Data;
using System.Globalization;

namespace DatabaseBenchmark.Reporting
{
    public class CsvReportFormatter : IReportFormatter
    {
        public void Print(Stream stream, DataTable results)
        {
            using var writer = new StreamWriter(stream);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            foreach (DataColumn column in results.Columns)
            {
                csv.WriteField(column.Caption);
            }

            csv.NextRecord();

            foreach (DataRow row in results.Rows)
            {
                foreach (DataColumn column in results.Columns)
                {
                    csv.WriteField(row[column]);
                }

                csv.NextRecord();
            }
        }
    }
}
