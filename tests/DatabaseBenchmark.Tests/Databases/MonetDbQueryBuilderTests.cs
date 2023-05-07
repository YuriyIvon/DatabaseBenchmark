using DatabaseBenchmark.Databases.MonetDb;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Model;
using DatabaseBenchmark.Tests.Utils;
using System.Linq;
using Xunit;

namespace DatabaseBenchmark.Tests.Databases
{
    public class MonetDbQueryBuilderTests
    {
        [Fact]
        public void BuildQueryNoArguments()
        {
            var parametersBuilder = new SqlParametersBuilder();
            var builder = new MonetDbQueryBuilder(SampleInputs.Table, SampleInputs.NoArgumentsQuery, parametersBuilder, null, null);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal(@"SELECT * FROM Sample", normalizedQueryText);
        }

        [Fact]
        public void BuildQueryAllArguments()
        {
            var query = SampleInputs.AllArgumentsQuery;
            var parametersBuilder = new SqlParametersBuilder();
            var builder = new MonetDbQueryBuilder(SampleInputs.Table, query, parametersBuilder, null, null);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal("SELECT Category, SubCategory, SUM(Price) TotalPrice FROM Sample"
                + " WHERE (Category = @p0 AND SubCategory IS NULL AND Rating >= @p1)"
                + " GROUP BY Category, SubCategory"
                + " ORDER BY Category ASC, SubCategory ASC"
                + " LIMIT @p2 OFFSET @p3", normalizedQueryText);

            var reference = new SqlQueryParameter[]
            {
                new ('@', "p0", "ABC", ColumnType.String),
                new ('@', "p1", 5.0, ColumnType.Double),
                new ('@', "p2", 100, ColumnType.Integer),
                new ('@', "p3", 10, ColumnType.Integer)
            };

            Assert.Equal(reference, parametersBuilder.Parameters);
        }
    }
}
