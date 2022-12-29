using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Model;
using DatabaseBenchmark.Tests.Utils;
using System;
using Xunit;

namespace DatabaseBenchmark.Tests.Databases
{
    public class SqlInsertBuilderTests
    {
        [Fact]
        public void BuildInsertSingleRowNoParameters()
        {
            var source = new SampleDataSource();
            var reader = new DataSourceReader(source);
            var parametersBuilder = new SqlNoParametersBuilder();
            var options = new InsertBuilderOptions { BatchSize = 1 };
            var queryBuilder = new SqlInsertBuilder(SampleInputs.Table, reader, parametersBuilder, options);

            var insertQuery = queryBuilder.Build().NormalizeSpaces();

            Assert.Equal("INSERT INTO Sample (Id, Category, SubCategory, Name, CreatedAt, Rating, Price, Count)"
                + " VALUES (1, 'Category', 'SubCategory', 'One', '2022-10-10T00:00:00.0000000', 5, 23.5, 50)",
                insertQuery);
            Assert.Empty(parametersBuilder.Parameters);
        }

        [Fact]
        public void BuildInsertSingleRowWithParameters()
        {
            var source = new SampleDataSource();
            var reader = new DataSourceReader(source);
            var parametersBuilder = new SqlParametersBuilder();
            var options = new InsertBuilderOptions { BatchSize = 1 };
            var queryBuilder = new SqlInsertBuilder(SampleInputs.Table, reader, parametersBuilder, options);

            var insertQuery = queryBuilder.Build().NormalizeSpaces();

            Assert.Equal("INSERT INTO Sample (Id, Category, SubCategory, Name, CreatedAt, Rating, Price, Count)"
                + " VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7)",
                insertQuery);

            var reference = new SqlQueryParameter[]
            {
                new ('@', "p0", 1, ColumnType.Integer),
                new ('@', "p1", "Category", ColumnType.String),
                new ('@', "p2", "SubCategory", ColumnType.String),
                new ('@', "p3", "One", ColumnType.String),
                new ('@', "p4", new DateTime(2022, 10, 10), ColumnType.DateTime),
                new ('@', "p5", 5.0, ColumnType.Double),
                new ('@', "p6", 23.5, ColumnType.Double),
                new ('@', "p7", 50, ColumnType.Integer)
            };

            Assert.Equal(reference, parametersBuilder.Parameters);
        }

        [Fact]
        public void BuildInsertMultipleRowsNoParameters()
        {
            var source = new SampleDataSource();
            var reader = new DataSourceReader(source);
            var parametersBuilder = new SqlNoParametersBuilder();
            var options = new InsertBuilderOptions { BatchSize = 3 };
            var queryBuilder = new SqlInsertBuilder(SampleInputs.Table, reader, parametersBuilder, options);

            var insertQuery = queryBuilder.Build().NormalizeSpaces();

            Assert.Equal("INSERT INTO Sample (Id, Category, SubCategory, Name, CreatedAt, Rating, Price, Count)"
                + " VALUES (1, 'Category', 'SubCategory', 'One', '2022-10-10T00:00:00.0000000', 5, 23.5, 50),"
                + " (2, 'Category', 'SubCategory', 'Two', '2022-10-14T00:00:00.0000000', 4.2, 57.1, 230),"
                + " (3, 'Category', 'SubCategory', 'Three', '2022-11-03T00:00:00.0000000', 3.8, 45.2, 10)",
                insertQuery);
            Assert.Empty(parametersBuilder.Parameters);
        }

        [Fact]
        public void BuildInsertNoMoreData()
        {
            var source = new SampleDataSource();
            var reader = new DataSourceReader(source);
            var parametersBuilder = new SqlParametersBuilder();
            var options = new InsertBuilderOptions { BatchSize = 1 };
            var builder = new SqlInsertBuilder(SampleInputs.Table, reader, parametersBuilder, options);

            reader.ReadDictionary(SampleInputs.Table.Columns, out var _);
            reader.ReadDictionary(SampleInputs.Table.Columns, out var _);
            reader.ReadDictionary(SampleInputs.Table.Columns, out var _);
            var insertQuery = builder.Build();

            Assert.Null(insertQuery);
        }
    }
}
