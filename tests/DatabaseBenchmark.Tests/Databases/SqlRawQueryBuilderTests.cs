using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Model;
using DatabaseBenchmark.Tests.Utils;
using System.Linq;
using Xunit;

namespace DatabaseBenchmark.Tests.Databases
{
    public class SqlRawQueryBuilderTests
    {
        [Fact]
        public void BuildParameterizedQuery()
        {
            var query = SampleInputs.RawSqlQuery;
            var parametersBuilder = new SqlQueryParametersBuilder();
            var builder = new SqlRawQueryBuilder(query, parametersBuilder, null);

            var queryText = builder.Build();

            Assert.Equal("SELECT * FROM Sample WHERE Category = @p0", queryText);

            var reference = new SqlQueryParameter[]
            {
                new ('@', "p0", "ABC", ColumnType.String)
            };

            Assert.Equal(reference, parametersBuilder.Parameters);
        }
    }
}
