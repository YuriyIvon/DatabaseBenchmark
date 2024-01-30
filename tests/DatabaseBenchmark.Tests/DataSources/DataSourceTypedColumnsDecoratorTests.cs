using DatabaseBenchmark.Common;
using DatabaseBenchmark.DataSources.Decorators;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit;

namespace DatabaseBenchmark.Tests.DataSources
{
    public class DataSourceTypedColumnsDecoratorTests
    {
        public static IEnumerable<object[]> CompatibleTypeSamples =>
        [
            [ColumnType.Boolean, true, true],
            [ColumnType.Boolean, "false", false],
            [ColumnType.Integer, 10, 10],
            [ColumnType.Integer, (short)10, 10],
            [ColumnType.Integer, (long)10, 10],
            [ColumnType.Integer, (ushort)10, 10],
            [ColumnType.Integer, (uint)10, 10],
            [ColumnType.Integer, (ulong)10, 10],
            [ColumnType.Integer, (float)10, 10],
            [ColumnType.Integer, (double)10, 10],
            [ColumnType.Integer, (decimal)10, 10],
            [ColumnType.Integer, "10", 10],
            [ColumnType.Long, (long)10, 10L],
            [ColumnType.Long, (short)10, 10L],
            [ColumnType.Long, 10, 10L],
            [ColumnType.Long, (ushort)10, 10L],
            [ColumnType.Long, (uint)10, 10L],
            [ColumnType.Long, (ulong)10, 10L],
            [ColumnType.Long, (float)10, 10L],
            [ColumnType.Long, (double)10, 10L],
            [ColumnType.Long, (decimal)10, 10L],
            [ColumnType.Long, "10", 10L],
            [ColumnType.Double, (double)10, 10D],
            [ColumnType.Double, (short)10, 10D],
            [ColumnType.Double, 10, 10D],
            [ColumnType.Double, (long)10, 10D],
            [ColumnType.Double, (ushort)10, 10D],
            [ColumnType.Double, (uint)10, 10D],
            [ColumnType.Double, (ulong)10, 10D],
            [ColumnType.Double, (float)10, 10D],
            [ColumnType.Double, (decimal)10, 10D],
            [ColumnType.Double, "10", 10D],
            [ColumnType.Double, double.NaN, null],
            [ColumnType.DateTime, "2024-02-01T12:13:14", new DateTime(2024, 2, 1, 12, 13, 14)],
            [ColumnType.DateTime, new DateTime(2024, 2, 1, 12, 13, 14), new DateTime(2024, 2, 1, 12, 13, 14)],
            [ColumnType.Guid, Guid.Parse("3d0dc68b-3a19-4b44-9143-84db02bfa9f8"), Guid.Parse("3d0dc68b-3a19-4b44-9143-84db02bfa9f8")],
            [ColumnType.Guid, "3d0dc68b-3a19-4b44-9143-84db02bfa9f8", Guid.Parse("3d0dc68b-3a19-4b44-9143-84db02bfa9f8")],
            [ColumnType.String, "abc", "abc"],
            [ColumnType.String, false, "False"],
            [ColumnType.String, 10, "10"],
            [ColumnType.String, new DateTime(2024, 2, 1, 12, 13, 14), "2024-02-01T12:13:14"],
            [ColumnType.String, Guid.Parse("3d0dc68b-3a19-4b44-9143-84db02bfa9f8"), "3d0dc68b-3a19-4b44-9143-84db02bfa9f8"]
        ];

        public static IEnumerable<object[]> IncompatibleTypeSamples =>
        [
            [ColumnType.Boolean, 10],
            [ColumnType.Boolean, "abc"],
            [ColumnType.Integer, false],
            [ColumnType.Integer, "abc"],
            [ColumnType.Long, false],
            [ColumnType.Long, "abc"],
            [ColumnType.Double, false],
            [ColumnType.Double, "abc"],
            [ColumnType.DateTime, false],
            [ColumnType.DateTime, 12],
            [ColumnType.DateTime, "abc"],
            [ColumnType.Guid, false],
            [ColumnType.Guid, 12],
            [ColumnType.Guid, "abc"]
        ];

        [Theory]
        [MemberData(nameof(CompatibleTypeSamples))]
        public void ReadCompatibleValue(ColumnType columnType, object sourceValue, object targetValue)
        {
            var columns = new Column[]
            {
                new() { Name = "Dummy", Type = columnType }
            };

            IDataSource dataSource = new TestDataSource(sourceValue);
            dataSource = new DataSourceTypedColumnsDecorator(dataSource, columns, CultureInfo.InvariantCulture);

            var value = dataSource.GetValue(columns[0].Name);

            Assert.Equal(targetValue, value);
        }

        [Theory]
        [MemberData(nameof(IncompatibleTypeSamples))]
        public void ReadIncompatibleValue(ColumnType columnType, object sourceValue)
        {
            var columns = new Column[]
            {
                new() { Name = "Dummy", Type = columnType }
            };

            IDataSource dataSource = new TestDataSource(sourceValue);
            dataSource = new DataSourceTypedColumnsDecorator(dataSource, columns, CultureInfo.InvariantCulture);

            Assert.Throws<InputArgumentException>(() => dataSource.GetValue(columns[0].Name));
        }

        private class TestDataSource : IDataSource
        {
            private readonly object _value;

            public TestDataSource(object sourceValue)
            {
                _value = sourceValue;
            }

            public object GetValue(string name) => _value;

            public bool Read() => true;

            public void Dispose()
            {
            }
        }
    }
}
