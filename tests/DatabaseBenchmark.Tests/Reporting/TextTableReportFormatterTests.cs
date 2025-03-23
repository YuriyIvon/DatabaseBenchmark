using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core;
using DatabaseBenchmark.Reporting;
using System;
using System.IO;
using Xunit;

namespace DatabaseBenchmark.Tests.Reporting
{
    public class TextTableReportFormatterTests
    {
        private const string NameColumn = "Name";
        private const string CountColumn = "Count";
        private const string OptionalColumn = "Optional";

        private readonly LightweightDataTable _sampleResults;

        public TextTableReportFormatterTests()
        {
            _sampleResults = new LightweightDataTable();
            _sampleResults.Columns.Add(new LightweightDataColumn(NameColumn, _sampleResults));
            _sampleResults.Columns.Add(new LightweightDataColumn(CountColumn, _sampleResults));
            _sampleResults.Columns.Add(new LightweightDataColumn(OptionalColumn, _sampleResults));
            var row = _sampleResults.AddRow();
            row[NameColumn] = "Regular row";
            row[CountColumn] = 1;
            row = _sampleResults.AddRow();
            row[NameColumn] = "Optional attribute row";
            row[CountColumn] = 12;
            row[OptionalColumn] = 123.4567;
        }

        [Fact]
        public void PrintTable()
        {
            var tableFormatter = new TextTableReportFormatter(new ValueFormatter());
            using var stream = new MemoryStream();

            tableFormatter.Print(stream, _sampleResults);

            stream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(stream);
            var result = reader.ReadToEnd();

            var expectedText = "+------------------------+-----+--------+" + Environment.NewLine +
                               "|                    Name|Count|Optional|" + Environment.NewLine +
                               "+------------------------+-----+--------+" + Environment.NewLine +
                               "|\"Regular row\"           |    1|NULL    |" + Environment.NewLine +
                               "|\"Optional attribute row\"|   12|  123.46|" + Environment.NewLine +
                               "+------------------------+-----+--------+" + Environment.NewLine + Environment.NewLine;

            Assert.Equal(expectedText, result);
        }
    }
}
