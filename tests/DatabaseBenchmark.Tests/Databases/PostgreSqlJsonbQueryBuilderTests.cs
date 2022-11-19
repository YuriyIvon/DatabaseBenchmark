using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.PostgreSql;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Tests.Utils;
using NSubstitute;
using Xunit;

namespace DatabaseBenchmark.Tests.Databases
{
    public class PostgreSqlJsonbQueryBuilderTests
    {
        private readonly IOptionsProvider _optionsProvider;

        public PostgreSqlJsonbQueryBuilderTests()
        {
            _optionsProvider = Substitute.For<IOptionsProvider>();
            _optionsProvider.GetOptions<PostgreSqlJsonbQueryOptions>().Returns(new PostgreSqlJsonbQueryOptions());
        }

        [Fact]
        public void BuildQueryNoArguments()
        {
            var parametersBuilder = new SqlParametersBuilder();
            var builder = new PostgreSqlJsonbQueryBuilder(SampleInputs.Table, SampleInputs.NoArgumentsQuery, parametersBuilder, null, null, _optionsProvider);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal(@"SELECT * FROM Sample", normalizedQueryText);
        }

        [Fact]
        public void BuildQuerySelectSpecificFields()
        {
            var parametersBuilder = new SqlParametersBuilder();
            var builder = new PostgreSqlJsonbQueryBuilder(SampleInputs.Table, SampleInputs.SpecificFieldsQuery, parametersBuilder, null, null, _optionsProvider);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal(@"SELECT (attributes->>'Id')::integer Id, "
                + "attributes->>'Category' Category, "
                + "attributes->>'Name' Name, "
                + "attributes->>'CreatedAt' CreatedAt, "
                + "(attributes->>'Rating')::double Rating, "
                + "Price Price FROM Sample",
                normalizedQueryText);
        }

        [Fact]
        public void BuildQueryAllArgumentsGinOperators()
        {
            var query = SampleInputs.AllArgumentsQuery;
            var parametersBuilder = new SqlParametersBuilder();
            var builder = new PostgreSqlJsonbQueryBuilder(SampleInputs.Table, query, parametersBuilder, null, null, _optionsProvider);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal("SELECT attributes->>'Category' Category, attributes->>'SubCategory' SubCategory, SUM(Price) TotalPrice FROM Sample"
                + " WHERE (attributes @> '{\"Category\": \"ABC\"}'::jsonb AND attributes @> '{\"SubCategory\": null}'::jsonb)"
                + " GROUP BY attributes->>'Category', attributes->>'SubCategory'"
                + " ORDER BY attributes->>'Category' ASC, attributes->>'SubCategory' ASC"
                + " OFFSET @p0 ROWS FETCH NEXT @p1 ROWS ONLY", normalizedQueryText);
            Assert.Equal(2, parametersBuilder.Values.Count);
            Assert.Equal(query.Skip, parametersBuilder.Values["@p0"]);
            Assert.Equal(query.Take, parametersBuilder.Values["@p1"]);
        }

        [Fact]
        public void BuildQueryAllArgumentsNoGinOperators()
        {
            var query = SampleInputs.AllArgumentsQuery;
            var parametersBuilder = new SqlParametersBuilder();
            _optionsProvider.GetOptions<PostgreSqlJsonbQueryOptions>().Returns(new PostgreSqlJsonbQueryOptions {  UseGinOperators = false });
            var builder = new PostgreSqlJsonbQueryBuilder(SampleInputs.Table, query, parametersBuilder, null, null, _optionsProvider);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal("SELECT attributes->>'Category' Category, attributes->>'SubCategory' SubCategory, SUM(Price) TotalPrice FROM Sample"
                + " WHERE (attributes->>'Category' = @p0 AND attributes->>'SubCategory' IS NULL)"
                + " GROUP BY attributes->>'Category', attributes->>'SubCategory'"
                + " ORDER BY attributes->>'Category' ASC, attributes->>'SubCategory' ASC"
                + " OFFSET @p1 ROWS FETCH NEXT @p2 ROWS ONLY", normalizedQueryText);
            Assert.Equal(3, parametersBuilder.Values.Count);
            Assert.Equal("ABC", parametersBuilder.Values["@p0"]);
            Assert.Equal(query.Skip, parametersBuilder.Values["@p1"]);
            Assert.Equal(query.Take, parametersBuilder.Values["@p2"]);
        }
    }
}
