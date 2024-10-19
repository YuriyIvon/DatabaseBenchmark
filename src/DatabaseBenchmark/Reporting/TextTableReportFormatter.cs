using DatabaseBenchmark.Common;
using DatabaseBenchmark.Reporting.Interfaces;

namespace DatabaseBenchmark.Reporting
{
    public class TextTableReportFormatter : IReportFormatter
    {
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

        private static int GetColumnWidth(LightweightDataColumn column)
        {
            var values = column.Table.Rows
                .Where(r => r[column.Name] != DBNull.Value)
                .Select(r => FormatRawValue(r[column.Name]))
                .ToArray();

            var maxLength = values.Any() ? values.Max(v => v.Length) : 0;
            var captionLength = column.Caption?.Length ?? 0;

            return Math.Max(maxLength, captionLength);
        }

        private static string FormatValue(LightweightDataColumn column, LightweightDataRow row)
        {
            var columnWidth = GetColumnWidth(column);
            var value = row[column.Name];
            var unpaddedValue = FormatRawValue(value);

            return value switch
            {
                _ when IsNumber(value) => unpaddedValue.PadLeft(columnWidth),
                _ => unpaddedValue.PadRight(columnWidth)
            };
        }

        private static string FormatRawValue(object value) =>
            value switch
            {
                IEnumerable<object> arrayValue => $"[{string.Join(", ", arrayValue.Select(FormatRawValue))}]",
                DateTime dateTimeValue => dateTimeValue.ToString("o"), // TODO: Make output date/time format configurable
                DateTimeOffset dateTimeOffsetValue => dateTimeOffsetValue.ToString("o"), // TODO: Make output date/time format configurable
                double doubleValue => string.Format($"{value:0.##}"), // TODO: Make precision configurable
                float floatValue => string.Format($"{value:0.##}"), // TODO: Make precision configurable
                _ when IsNumber(value) => value.ToString(),
                _ => $"\"{value}\"" // TODO: Make quotemarks configurable
            };

        private static bool IsNumber(object value) => value is byte or short or ushort or int or uint or long or ulong or double or float;
    }
}
