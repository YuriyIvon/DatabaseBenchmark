using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.PostgreSql;
using DatabaseBenchmark.Databases.Sql;
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
                + " WHERE @p0 = ANY(Tags)", normalizedQueryText);
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
                + " WHERE Tags @> '{\"ABC\"}'", normalizedQueryText);
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
            var optionsProvider = Substitute.For<IOptionsProvider>();
            optionsProvider.GetOptions<PostgreSqlQueryOptions>().Returns(new PostgreSqlQueryOptions());
            var builder = new PostgreSqlQueryBuilder(SampleInputs.ArrayColumnTable, query, parametersBuilder, null, null, optionsProvider);

            Assert.Throws<InputArgumentException>(builder.Build);
        }
    }
}
