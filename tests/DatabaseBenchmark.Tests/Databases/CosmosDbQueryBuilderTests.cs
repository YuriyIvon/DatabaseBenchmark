using DatabaseBenchmark.Databases.CosmosDb;
using DatabaseBenchmark.Databases.Sql;
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
            var builder = new CosmosDbQueryBuilder(SampleInputs.Table, SampleInputs.NoArgumentsQuery, parametersBuilder, null);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal(@"SELECT * FROM Sample", normalizedQueryText);
        }

        [Fact]
        public void BuildQueryAllArguments()
        {
            var query = SampleInputs.AllArgumentsQuery;
            var parametersBuilder = new SqlParametersBuilder();
            var builder = new CosmosDbQueryBuilder(SampleInputs.Table, query, parametersBuilder, null);

            var queryText = builder.Build();

            //Please note than in fact in CosmosDB GROUP BY and ORDER BY are mutually exclusive
            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal("SELECT Sample.Category, Sample.SubCategory, SUM(Sample.Price) TotalPrice FROM Sample"
                + " WHERE (Sample.Category = @p0 AND IS_NULL(Sample.SubCategory))"
                + " GROUP BY Sample.Category, Sample.SubCategory"
                + " ORDER BY Sample.Category ASC, Sample.SubCategory ASC"
                + " OFFSET @p1 LIMIT @p2", normalizedQueryText);
            Assert.Equal(3, parametersBuilder.Values.Count);
            Assert.Equal("ABC", parametersBuilder.Values["@p0"]);
            Assert.Equal(10, parametersBuilder.Values["@p1"]);
            Assert.Equal(100, parametersBuilder.Values["@p2"]);
        }
    }
}
