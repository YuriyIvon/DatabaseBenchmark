using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;
using DatabaseBenchmark.Tests.Utils;
using System;
using Xunit;

namespace DatabaseBenchmark.Tests.Databases
{
    public class SqlRawQueryBuilderTests
    {
        [Fact]
        public void BuildParameterizedQuery()
        {
            var query = SampleInputs.RawSqlQuery;
            var parametersBuilder = new SqlParametersBuilder();
            var builder = new SqlRawQueryBuilder(query, parametersBuilder, null);

            var queryText = builder.Build();

            Assert.Equal("SELECT * FROM Sample WHERE Category = @p0 AND CreatedDate >= @p1 AND Price <= @p2 AND Available = @p3 AND UserId <> @p4", queryText);

            var reference = new SqlQueryParameter[]
            {
                new ('@', "p0", "ABC", ColumnType.String),
                new ('@', "p1", new DateTime(2020, 1, 2, 3, 4, 5), ColumnType.DateTime),
                new ('@', "p2", 25.5, ColumnType.Double),
                new ('@', "p3", true, ColumnType.Boolean),
                new ('@', "p4", "d5a611c6-aa28-4842-8643-6a58e2f8123e", ColumnType.Guid)
            };

            Assert.Equal(reference, parametersBuilder.Parameters);
        }

        [Fact]
        public void BuildInlineParameterizedQuery()
        {
            var query = SampleInputs.RawSqlInlineQuery;
            var parametersBuilder = new SqlParametersBuilder();
            var builder = new SqlRawQueryBuilder(query, parametersBuilder, null);

            var queryText = builder.Build();

            Assert.Equal("SELECT * FROM Sample WHERE Category = 'ABC' AND CreatedDate >= '2020-01-02T03:04:05.0000000' AND Price <= 25.5", queryText);
            Assert.Empty(parametersBuilder.Parameters);
        }
    }
}
