using DatabaseBenchmark.Common;
using DatabaseBenchmark.Databases.ClickHouse;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;
using DatabaseBenchmark.Tests.Utils;
using Xunit;

namespace DatabaseBenchmark.Tests.Databases
{
    public class ClickHouseQueryBuilderTests
    {
        [Fact]
        public void BuildQueryNoArguments()
        {
            var parametersBuilder = new SqlParametersBuilder();
            var builder = new ClickHouseQueryBuilder(SampleInputs.Table, SampleInputs.NoArgumentsQuery, parametersBuilder, null, null);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal(@"SELECT * FROM Sample", normalizedQueryText);
        }

        [Fact]
        public void BuildQueryAllArguments()
        {
            var query = SampleInputs.AllArgumentsQuery;
            var parametersBuilder = new SqlParametersBuilder();
            var builder = new ClickHouseQueryBuilder(SampleInputs.Table, query, parametersBuilder, null, null);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal("SELECT Category, SubCategory, SUM(Price) TotalPrice FROM Sample"
                + " WHERE (Category = @p0 AND SubCategory IS NULL AND Rating >= @p1 AND (Name LIKE @p2 OR Name LIKE @p3))"
                + " GROUP BY Category, SubCategory"
                + " ORDER BY Category ASC, SubCategory ASC"
                + " LIMIT 100 OFFSET 10", normalizedQueryText);

            var reference = new SqlQueryParameter[]
            {
                new ('@', "p0", "ABC", ColumnType.String),
                new ('@', "p1", 5.0, ColumnType.Double),
                new ('@', "p2", "A%", ColumnType.String),
                new ('@', "p3", "%B%", ColumnType.String)
            };

            Assert.Equal(reference, parametersBuilder.Parameters);
        }

        [Fact]
        public void BuildQueryArrayColumn()
        {
            var query = SampleInputs.ArrayColumnQuery;
            var parametersBuilder = new SqlParametersBuilder();
            var builder = new ClickHouseQueryBuilder(SampleInputs.ArrayColumnTable, query, parametersBuilder, null, null);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal("SELECT Category, SubCategory FROM Sample"
                + " WHERE has(Tags, @p0)", normalizedQueryText);
        }

        [Theory]
        [InlineData(QueryPrimitiveOperator.In)]
        [InlineData(QueryPrimitiveOperator.Greater)]
        [InlineData(QueryPrimitiveOperator.GreaterEquals)]
        [InlineData(QueryPrimitiveOperator.Lower)]
        [InlineData(QueryPrimitiveOperator.LowerEquals)]
        [InlineData(QueryPrimitiveOperator.StartsWith)]
        public void BuildQueryArrayColumnUnsupportedOperator(QueryPrimitiveOperator @operator)
        {
            var query = SampleInputs.ArrayColumnQuery;
            ((QueryPrimitiveCondition)query.Condition).Operator = @operator;

            var parametersBuilder = new SqlParametersBuilder();
            var builder = new ClickHouseQueryBuilder(SampleInputs.ArrayColumnTable, query, parametersBuilder, null, null);

            Assert.Throws<InputArgumentException>(builder.Build);
        }
    }
}
