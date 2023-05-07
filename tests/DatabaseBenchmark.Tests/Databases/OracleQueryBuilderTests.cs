using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Model;
using DatabaseBenchmark.Tests.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DatabaseBenchmark.Tests.Databases
{
    public class OracleQueryBuilderTests
    {
        [Fact]
        public void BuildQueryNoArguments()
        {
            var parametersBuilder = new SqlParametersBuilder(':');
            var builder = new SqlQueryBuilder(SampleInputs.Table, SampleInputs.NoArgumentsQuery, parametersBuilder, null, null);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal(@"SELECT * FROM Sample", normalizedQueryText);
        }

        [Fact]
        public void BuildQueryAllArguments()
        {
            var query = SampleInputs.AllArgumentsQuery;
            var parametersBuilder = new SqlParametersBuilder(':');
            var builder = new SqlQueryBuilder(SampleInputs.Table, query, parametersBuilder, null, null);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal("SELECT Category, SubCategory, SUM(Price) TotalPrice FROM Sample"
                + " WHERE (Category = :p0 AND SubCategory IS NULL AND Rating >= :p1)"
                + " GROUP BY Category, SubCategory"
                + " ORDER BY Category ASC, SubCategory ASC"
                + " OFFSET :p2 ROWS FETCH NEXT :p3 ROWS ONLY", normalizedQueryText);

            var reference = new SqlQueryParameter[]
            {
                new (':', "p0", "ABC", ColumnType.String),
                new (':', "p1", 5.0, ColumnType.Double),
                new (':', "p2", 10, ColumnType.Integer),
                new (':', "p3", 100, ColumnType.Integer)
            };

            Assert.Equal(reference, parametersBuilder.Parameters);
        }
    }
}
