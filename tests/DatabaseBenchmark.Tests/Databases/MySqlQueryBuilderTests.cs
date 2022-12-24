using DatabaseBenchmark.Databases.MySql;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Model;
using DatabaseBenchmark.Tests.Utils;
using System.Linq;
using Xunit;

namespace DatabaseBenchmark.Tests.Databases
{
    public class MySqlQueryBuilderTests
    {
        [Fact]
        public void BuildQueryNoArguments()
        {
            var parametersBuilder = new SqlQueryParametersBuilder();
            var builder = new MySqlQueryBuilder(SampleInputs.Table, SampleInputs.NoArgumentsQuery, parametersBuilder, null, null);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal(@"SELECT * FROM Sample", normalizedQueryText);
        }

        [Fact]
        public void BuildQueryAllArguments()
        {
            var query = SampleInputs.AllArgumentsQuery;
            var parametersBuilder = new SqlQueryParametersBuilder();
            var builder = new MySqlQueryBuilder(SampleInputs.Table, query, parametersBuilder, null, null);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal("SELECT Category, SubCategory, SUM(Price) TotalPrice FROM Sample"
                + " WHERE (Category = @p0 AND SubCategory IS NULL)"
                + " GROUP BY Category, SubCategory"
                + " ORDER BY Category ASC, SubCategory ASC"
                + " LIMIT 10, 100", normalizedQueryText);

            var reference = new SqlQueryParameter[]
            {
                new ('@', "p0", "ABC", ColumnType.String)
            };

            Assert.Equal(reference, parametersBuilder.Parameters);
        }
    }
}
