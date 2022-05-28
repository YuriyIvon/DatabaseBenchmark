using DatabaseBenchmark.Databases.PostgreSql;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Tests.Utils;
using Xunit;

namespace DatabaseBenchmark.Tests.Databases
{
    public class PostgreSqlJsonbQueryBuilderTests
    {
        [Fact]
        public void BuildQueryNoArguments()
        {
            var parametersBuilder = new SqlParametersBuilder();
            var builder = new PostgreSqlJsonbQueryBuilder(SampleInputs.Table, SampleInputs.NoArgumentsQuery, parametersBuilder, null);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal(@"SELECT * FROM Sample", normalizedQueryText);
        }

        [Fact]
        public void BuildQueryAllArguments()
        {
            var query = SampleInputs.AllArgumentsQuery;
            var parametersBuilder = new SqlParametersBuilder();
            var builder = new PostgreSqlJsonbQueryBuilder(SampleInputs.Table, query, parametersBuilder, null);

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
    }
}
