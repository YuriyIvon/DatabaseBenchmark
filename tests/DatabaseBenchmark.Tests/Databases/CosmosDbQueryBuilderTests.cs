using DatabaseBenchmark.Common;
using DatabaseBenchmark.Databases.CosmosDb;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;
using DatabaseBenchmark.Tests.Utils;
using Xunit;

namespace DatabaseBenchmark.Tests.Databases
{
    public class CosmosDbQueryBuilderTests
    {
        [Fact]
        public void BuildQueryNoArguments()
        {
            var parametersBuilder = new SqlParametersBuilder();
            var builder = new CosmosDbQueryBuilder(SampleInputs.Table, SampleInputs.NoArgumentsQuery, parametersBuilder, null, null);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal(@"SELECT * FROM Sample", normalizedQueryText);
        }

        [Fact]
        public void BuildQueryAllArguments()
        {
            var query = SampleInputs.AllArgumentsQuery;
            var parametersBuilder = new SqlParametersBuilder();
            var builder = new CosmosDbQueryBuilder(SampleInputs.Table, query, parametersBuilder, null, null);

            var queryText = builder.Build();

            //Please note than in fact in CosmosDB GROUP BY and ORDER BY are mutually exclusive
            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal("SELECT Sample.Category, Sample.SubCategory, SUM(Sample.Price) TotalPrice FROM Sample"
                + " WHERE (Sample.Category IN (@p0, @p1) AND IS_NULL(Sample.SubCategory) AND Sample.Rating >= @p2 AND Sample.Count = @p3 AND (Sample.Name LIKE @p4 OR Sample.Name LIKE @p5))"
                + " GROUP BY Sample.Category, Sample.SubCategory"
                + " ORDER BY Sample.Category ASC, Sample.SubCategory ASC"
                + " OFFSET @p6 LIMIT @p7", normalizedQueryText);

            var reference = new SqlQueryParameter[]
            {
                new ('@', "p0", "ABC", ColumnType.String),
                new ('@', "p1", "DEF", ColumnType.String),
                new ('@', "p2", 5.0, ColumnType.Double),
                new ('@', "p3", 0, ColumnType.Integer),
                new ('@', "p4", "A%", ColumnType.String),
                new ('@', "p5", "%B%", ColumnType.String),
                new ('@', "p6", 10, ColumnType.Integer),
                new ('@', "p7", 100, ColumnType.Integer)
            };

            Assert.Equal(reference, parametersBuilder.Parameters);
        }

        [Fact]
        public void BuildQueryArrayColumn()
        {
            var query = SampleInputs.ArrayColumnQuery;
            var parametersBuilder = new SqlParametersBuilder();
            var builder = new CosmosDbQueryBuilder(SampleInputs.ArrayColumnTable, query, parametersBuilder, null, null);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal("SELECT Sample.Category, Sample.SubCategory FROM Sample"
                + " WHERE (ARRAY_CONTAINS(Sample.Tags, @p0) OR Sample.Tags = @p1)", normalizedQueryText);

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
            var builder = new CosmosDbQueryBuilder(SampleInputs.ArrayColumnTable, query, parametersBuilder, null, null);

            Assert.Throws<InputArgumentException>(builder.Build);
        }
    }
}
