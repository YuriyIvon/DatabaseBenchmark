using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.MongoDb;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Tests.Utils;
using NSubstitute;
using Xunit;

namespace DatabaseBenchmark.Tests.Databases
{
    public class SqlQueryBuilderTests
    {
        [Fact]
        public void BuildQueryNoArguments()
        {
            var parametersBuilder = new SqlParametersBuilder();
            var builder = new SqlQueryBuilder(SampleInputs.Table, SampleInputs.NoArgumentsQuery, parametersBuilder, null, null);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal(@"SELECT * FROM Sample", normalizedQueryText);
        }

        [Fact]
        public void BuildQueryAllArguments()
        {
            var query = SampleInputs.AllArgumentsQuery;
            var parametersBuilder = new SqlParametersBuilder();
            var builder = new SqlQueryBuilder(SampleInputs.Table, query, parametersBuilder, null, null);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal("SELECT Category, SubCategory, SUM(Price) TotalPrice FROM Sample"
                + " WHERE (Category = @p0 AND SubCategory IS NULL)"
                + " GROUP BY Category, SubCategory"
                + " ORDER BY Category ASC, SubCategory ASC"
                + " OFFSET @p1 ROWS FETCH NEXT @p2 ROWS ONLY", normalizedQueryText);
            Assert.Equal(3, parametersBuilder.Values.Count);
            Assert.Equal("ABC", parametersBuilder.Values["@p0"]);
            Assert.Equal(query.Skip, parametersBuilder.Values["@p1"]);
            Assert.Equal(query.Take, parametersBuilder.Values["@p2"]);
        }

        [Fact]
        public void BuildQueryAllArgumentsIncludeNone()
        {
            var query = SampleInputs.AllArgumentsQueryRandomizeInclusionAll;

            var mockRandomValueProvider = Substitute.For<IRandomGenerator>();
            mockRandomValueProvider.GetRandomBoolean().Returns(true);
            var parametersBuilder = new SqlParametersBuilder();
            var builder = new SqlQueryBuilder(SampleInputs.Table, query, parametersBuilder, null, mockRandomValueProvider);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal("SELECT Category, SubCategory, SUM(Price) TotalPrice FROM Sample"
                + " GROUP BY Category, SubCategory"
                + " ORDER BY Category ASC, SubCategory ASC"
                + " OFFSET @p0 ROWS FETCH NEXT @p1 ROWS ONLY", normalizedQueryText);
            Assert.Equal(2, parametersBuilder.Values.Count);
            Assert.Equal(query.Skip, parametersBuilder.Values["@p0"]);
            Assert.Equal(query.Take, parametersBuilder.Values["@p1"]);
        }

        [Fact]
        public void BuildQueryAllArgumentsIncludePartial()
        {
            var query = SampleInputs.AllArgumentsQueryRandomizeInclusionPartial;

            var mockRandomValueProvider = Substitute.For<IRandomGenerator>();
            mockRandomValueProvider.GetRandomBoolean().Returns(true);
            var parametersBuilder = new SqlParametersBuilder();
            var builder = new SqlQueryBuilder(SampleInputs.Table, query, parametersBuilder, null, mockRandomValueProvider);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal("SELECT Category, SubCategory, SUM(Price) TotalPrice FROM Sample"
                + " WHERE (SubCategory IS NULL)"
                + " GROUP BY Category, SubCategory"
                + " ORDER BY Category ASC, SubCategory ASC"
                + " OFFSET @p0 ROWS FETCH NEXT @p1 ROWS ONLY", normalizedQueryText);
            Assert.Equal(2, parametersBuilder.Values.Count);
            Assert.Equal(query.Skip, parametersBuilder.Values["@p0"]);
            Assert.Equal(query.Take, parametersBuilder.Values["@p1"]);
        }
    }
}
