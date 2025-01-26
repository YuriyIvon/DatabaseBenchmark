using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.PostgreSql;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;
using DatabaseBenchmark.Tests.Utils;
using NSubstitute;
using Xunit;

namespace DatabaseBenchmark.Tests.Databases
{
    public class PostgreSqlQueryBuilderTests
    {
        [Fact]
        public void BuildQueryArrayColumn()
        {
            var query = SampleInputs.ArrayColumnQuery;
            var parametersBuilder = new SqlParametersBuilder();
            var optionsProvider = Substitute.For<IOptionsProvider>();
            optionsProvider.GetOptions<PostgreSqlQueryOptions>().Returns(new PostgreSqlQueryOptions());
            var builder = new PostgreSqlQueryBuilder(SampleInputs.ArrayColumnTable, query, parametersBuilder, null, null, optionsProvider);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal("SELECT Category, SubCategory FROM Sample"
                + " WHERE (@p0 = ANY(Tags) OR Tags = @p1)", normalizedQueryText);

            var reference = new SqlQueryParameter[]
            {
                new ('@', "p0", "ABC", ColumnType.String),
                new ('@', "p1", new object[] { "A", "B", "C" }, ColumnType.String, true)
            };

            Assert.Equal(reference, parametersBuilder.Parameters, new SqlQueryParameterEqualityComparer());
        }

        [Fact]
        public void BuildQueryArrayColumnGinOperator()
        {
            var query = SampleInputs.ArrayColumnQuery;
            var parametersBuilder = new SqlParametersBuilder();
            var optionsProvider = Substitute.For<IOptionsProvider>();
            optionsProvider.GetOptions<PostgreSqlQueryOptions>().Returns(new PostgreSqlQueryOptions { UseArrayGinOperators = true });
            var builder = new PostgreSqlQueryBuilder(SampleInputs.ArrayColumnTable, query, parametersBuilder, null, null, optionsProvider);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal("SELECT Category, SubCategory FROM Sample"
                + " WHERE (Tags @> '{\"ABC\"}' OR Tags = @p0)", normalizedQueryText);
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
            var optionsProvider = Substitute.For<IOptionsProvider>();
            optionsProvider.GetOptions<PostgreSqlQueryOptions>().Returns(new PostgreSqlQueryOptions());
            var builder = new PostgreSqlQueryBuilder(SampleInputs.ArrayColumnTable, query, parametersBuilder, null, null, optionsProvider);

            Assert.Throws<InputArgumentException>(builder.Build);
        }
    }
}
