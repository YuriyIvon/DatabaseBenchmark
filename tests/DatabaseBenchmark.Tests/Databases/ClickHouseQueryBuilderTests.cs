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
                + " WHERE (Category IN (@p0, @p1) AND SubCategory IS NULL AND Rating >= @p2 AND Count = @p3 AND (Name LIKE @p4 OR Name LIKE @p5))"
                + " GROUP BY Category, SubCategory"
                + " ORDER BY Category ASC, SubCategory ASC"
                + " LIMIT 100 OFFSET 10", normalizedQueryText);

            var reference = new SqlQueryParameter[]
            {
                new ('@', "p0", "ABC", ColumnType.String),
                new ('@', "p1", "DEF", ColumnType.String),
                new ('@', "p2", 5.0, ColumnType.Double),
                new ('@', "p3", 0, ColumnType.Integer),
                new ('@', "p4", "A%", ColumnType.String),
                new ('@', "p5", "%B%", ColumnType.String)
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
                + " WHERE (has(Tags, @p0) OR Tags = @p1)", normalizedQueryText);

            var reference = new SqlQueryParameter[]
            {
                new ('@', "p0", "ABC", ColumnType.String),
                new ('@', "p1", new object[] { "A", "B", "C" }, ColumnType.String, true)
            };

            Assert.Equal(reference, parametersBuilder.Parameters, new SqlQueryParameterEqualityComparer());
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

            //TODO: change to a query generator function that returns a query with the specified operator
            ((QueryPrimitiveCondition)((QueryGroupCondition)query.Condition).Conditions[0]).Operator = @operator;

            var parametersBuilder = new SqlParametersBuilder();
            var builder = new ClickHouseQueryBuilder(SampleInputs.ArrayColumnTable, query, parametersBuilder, null, null);

            Assert.Throws<InputArgumentException>(builder.Build);
        }
    }
}
