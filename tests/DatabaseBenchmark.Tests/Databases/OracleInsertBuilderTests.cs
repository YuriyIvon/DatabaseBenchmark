using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Oracle;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Tests.Utils;
using Xunit;

namespace DatabaseBenchmark.Tests.Databases
{
    public class OracleInsertBuilderTests
    {
        [Fact]
        public void BuildInsertMultipleRowsNoParameters()
        {
            var source = new SampleDataSource();
            var reader = new DataSourceReader(source);
            var parametersBuilder = new SqlNoParametersBuilder();
            var options = new InsertBuilderOptions { BatchSize = 3 };
            var queryBuilder = new OracleInsertBuilder(SampleInputs.Table, reader, parametersBuilder, options);

            var insertQuery = queryBuilder.Build().NormalizeSpaces();

            Assert.Equal("INSERT ALL INTO Sample (Id, Category, SubCategory, Name, CreatedAt, Rating, Price, Count)"
                + " VALUES (1, 'Category', 'SubCategory', 'One', '2022-10-10T00:00:00.0000000', 5, 23.5, 50)"
                + " INTO Sample (Id, Category, SubCategory, Name, CreatedAt, Rating, Price, Count)"
                + " VALUES (2, 'Category', 'SubCategory', 'Two', '2022-10-14T00:00:00.0000000', 4.2, 57.1, 230)"
                + " INTO Sample (Id, Category, SubCategory, Name, CreatedAt, Rating, Price, Count)"
                + " VALUES (3, 'Category', 'SubCategory', 'Three', '2022-11-03T00:00:00.0000000', 3.8, 45.2, 10)"
                + " SELECT 1 FROM DUAL",
                insertQuery);
            Assert.Empty(parametersBuilder.Parameters);
        }
    }
}
