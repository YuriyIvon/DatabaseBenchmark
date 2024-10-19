using CsvHelper;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Reporting.Interfaces;
using System.Globalization;

namespace DatabaseBenchmark.Reporting
{
    public class CsvReportFormatter : IReportFormatter
    {
        public void Print(Stream stream, LightweightDataTable results)
        {
            using var writer = new StreamWriter(stream);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            foreach (LightweightDataColumn column in results.Columns)
            {
                csv.WriteField(column.Caption);
            }

            csv.NextRecord();

            foreach (LightweightDataRow row in results.Rows)
            {
                foreach (LightweightDataColumn column in results.Columns)
                {
                    csv.WriteField(row[column.Name]);
                }

                csv.NextRecord();
            }
        }
    }
}
