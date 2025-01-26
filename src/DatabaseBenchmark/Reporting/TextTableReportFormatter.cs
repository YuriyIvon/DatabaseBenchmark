using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Reporting.Interfaces;
using System.Collections;

namespace DatabaseBenchmark.Reporting
{
    public class TextTableReportFormatter : IReportFormatter
    {
        private readonly IValueFormatter _valueFormatter;

        public TextTableReportFormatter(IValueFormatter valueFormatter)
        {
            _valueFormatter = valueFormatter;
        }

        public void Print(Stream stream, LightweightDataTable results)
        {
            using var writer = new StreamWriter(stream);

            foreach (var column in results.Columns)
            {
                writer.Write('+');

                var columnWidth = GetColumnWidth(column);
                writer.Write(new string('-', columnWidth));
            }

            writer.WriteLine('+');

            foreach (var column in results.Columns)
            {
                writer.Write('|');

                var columnWidth = GetColumnWidth(column);
                writer.Write(column.Caption.PadLeft(columnWidth));
            }

            writer.WriteLine('|');

            foreach (var column in results.Columns)
            {
                writer.Write('+');

                var columnWidth = GetColumnWidth(column);
                writer.Write(new string('-', columnWidth));
            }

            writer.WriteLine('+');

            foreach (var row in results.Rows)
            {
                foreach (var column in results.Columns)
                {
                    writer.Write('|');
                    writer.Write(FormatValue(column, row));
                }

                writer.WriteLine('|');
            }

            foreach (var column in results.Columns)
            {
                writer.Write('+');

                var columnWidth = GetColumnWidth(column);
                writer.Write(new string('-', columnWidth));
            }

            writer.WriteLine('+');
            writer.WriteLine();
        }

        private int GetColumnWidth(LightweightDataColumn column)
        {
            var values = column.Table.Rows
                .Where(r => r[column.Name] != DBNull.Value)
                .Select(r => _valueFormatter.Format(r[column.Name]))
                .ToArray();

            var maxLength = values.Any() ? values.Max(v => v.Length) : 0;
            var captionLength = column.Caption?.Length ?? 0;

            return Math.Max(maxLength, captionLength);
        }

        private string FormatValue(LightweightDataColumn column, LightweightDataRow row)
        {
            var columnWidth = GetColumnWidth(column);
            var value = row[column.Name];
            var unpaddedValue = _valueFormatter.Format(value);

            return value switch
            {
                _ when IsNumber(value) => unpaddedValue.PadLeft(columnWidth),
                _ => unpaddedValue.PadRight(columnWidth)
            };
        }

        private static bool IsNumber(object value) => value is byte or short or ushort or int or uint or long or ulong or double or float;
    }
}
