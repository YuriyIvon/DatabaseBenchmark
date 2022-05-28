using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Tests.Utils;
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

            Assert.Equal("SELECT * FROM Sample WHERE Category = @p0", queryText);
            Assert.Single(parametersBuilder.Values);
            Assert.Equal("ABC", parametersBuilder.Values["@p0"]);
        }
    }
}
