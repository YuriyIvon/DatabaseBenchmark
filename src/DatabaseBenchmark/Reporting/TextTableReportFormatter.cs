using DatabaseBenchmark.Interfaces.Reporting;
using System.Data;

namespace DatabaseBenchmark.Reporting
{
    public class TextTableReportFormatter : IReportFormatter
    {
        public void Print(Stream stream, DataTable results)
        {
            using var writer = new StreamWriter(stream);

            foreach (DataColumn column in results.Columns)
            {
                writer.Write('+');

                var columnWidth = GetColumnWidth(column);
                writer.Write(new string('-', columnWidth));
            }

            writer.WriteLine('+');

            foreach (DataColumn column in results.Columns)
            {
                writer.Write('|');

                var columnWidth = GetColumnWidth(column);
                writer.Write(column.Caption.PadLeft(columnWidth));
            }

            writer.WriteLine('|');

            foreach (DataColumn column in results.Columns)
            {
                writer.Write('+');

                var columnWidth = GetColumnWidth(column);
                writer.Write(new string('-', columnWidth));
            }

            writer.WriteLine('+');

            foreach (DataRow row in results.Rows)
            {
                foreach (DataColumn column in results.Columns)
                {
                    writer.Write('|');
                    writer.Write(FormatValue(column, row));
                }

                writer.WriteLine('|');
            }

            foreach (DataColumn column in results.Columns)
            {
                writer.Write('+');

                var columnWidth = GetColumnWidth(column);
                writer.Write(new string('-', columnWidth));
            }

            writer.WriteLine('+');
            writer.WriteLine();
        }

        private static int GetColumnWidth(DataColumn column)
        {
            if (column.DataType == typeof(string))
            {
                var values = column.Table.Rows.Cast<DataRow>()
                    .Where(r => r[column] != DBNull.Value)
                    .Select(r => (string)r[column])
                    .ToArray();

                var maxLength = values.Any() ? values.Max(v => v.Length) : 0;
                var captionLength = column.Caption?.Length ?? 0;

                return Math.Max(maxLength, captionLength);
            }
            
            if (column.DataType == typeof(DateTime))
            {
                return 20;
            }

            return 10;
        }

        private static string FormatValue(DataColumn column, DataRow row)
        {
            //TODO: make formatting more robust by supporting more column types
            var columnWidth = GetColumnWidth(column);
            var value = row[column];
            if (value is string stringValue)
            {
                return stringValue.PadRight(columnWidth);
            }
            else if (value is DateTime dateTimeValue)
            {
                return dateTimeValue.ToString("u");
            }
            else
            {
                return string.Format($"{value:0.##}").PadLeft(columnWidth);
            }
        }
    }
}
