using DatabaseBenchmark.Databases.CosmosDb;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Model;
using DatabaseBenchmark.Tests.Utils;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DatabaseBenchmark.Tests.Databases
{
    public class CosmosDbQueryBuilderTests
    {
        [Fact]
        public void BuildQueryNoArguments()
        {
            var parametersBuilder = new SqlParametersBuilder();
            var builder = new CosmosDbQueryBuilder(SampleInputs.Table, SampleInputs.NoArgumentsQuery, parametersBuilder, null, null);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal(@"SELECT * FROM Sample", normalizedQueryText);
        }

        [Fact]
        public void BuildQueryAllArguments()
        {
            var query = SampleInputs.AllArgumentsQuery;
            var parametersBuilder = new SqlParametersBuilder();
            var builder = new CosmosDbQueryBuilder(SampleInputs.Table, query, parametersBuilder, null, null);

            var queryText = builder.Build();

            //Please note than in fact in CosmosDB GROUP BY and ORDER BY are mutually exclusive
            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal("SELECT Sample.Category, Sample.SubCategory, SUM(Sample.Price) TotalPrice FROM Sample"
                + " WHERE (Sample.Category = @p0 AND IS_NULL(Sample.SubCategory) AND Sample.Rating >= @p1 AND (Sample.Name LIKE @p2 OR Sample.Name LIKE @p3))"
                + " GROUP BY Sample.Category, Sample.SubCategory"
                + " ORDER BY Sample.Category ASC, Sample.SubCategory ASC"
                + " OFFSET @p4 LIMIT @p5", normalizedQueryText);

            var reference = new SqlQueryParameter[]
            {
                new ('@', "p0", "ABC", ColumnType.String),
                new ('@', "p1", 5.0, ColumnType.Double),
                new ('@', "p2", "A%", ColumnType.String),
                new ('@', "p3", "%B%", ColumnType.String),
                new ('@', "p4", 10, ColumnType.Integer),
                new ('@', "p5", 100, ColumnType.Integer)
            };

            Assert.Equal(reference, parametersBuilder.Parameters);
        }
    }
}
