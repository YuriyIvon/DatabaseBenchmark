using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.MongoDb;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Model;
using DatabaseBenchmark.Tests.Utils;
using NSubstitute;
using System.Linq;
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
                + " WHERE (Category = @p0 AND SubCategory IS NULL AND Rating >= @p1)"
                + " GROUP BY Category, SubCategory"
                + " ORDER BY Category ASC, SubCategory ASC"
                + " OFFSET @p2 ROWS FETCH NEXT @p3 ROWS ONLY", normalizedQueryText);

            var reference = new SqlQueryParameter[]
            {
                new ('@', "p0", "ABC", ColumnType.String),
                new ('@', "p1", 5.0, ColumnType.Double),
                new ('@', "p2", query.Skip, ColumnType.Integer),
                new ('@', "p3", query.Take, ColumnType.Integer)
            };

            Assert.Equal(reference, parametersBuilder.Parameters);
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

            var reference = new SqlQueryParameter[]
            {
                new ('@', "p0", query.Skip, ColumnType.Integer),
                new ('@', "p1", query.Take, ColumnType.Integer)
            };

            Assert.Equal(reference, parametersBuilder.Parameters);
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
                + " WHERE (SubCategory IS NULL AND Rating >= @p0)"
                + " GROUP BY Category, SubCategory"
                + " ORDER BY Category ASC, SubCategory ASC"
                + " OFFSET @p1 ROWS FETCH NEXT @p2 ROWS ONLY", normalizedQueryText);

            var reference = new SqlQueryParameter[]
            {
                new ('@', "p0", 5.0, ColumnType.Double),
                new ('@', "p1", query.Skip, ColumnType.Integer),
                new ('@', "p2", query.Take, ColumnType.Integer),
            };

            Assert.Equal(reference, parametersBuilder.Parameters);
        }
    }
}
