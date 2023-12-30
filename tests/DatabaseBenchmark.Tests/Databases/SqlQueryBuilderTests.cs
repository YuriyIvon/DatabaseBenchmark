using DatabaseBenchmark.Common;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Model;
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
        public void BuildQueryNoArgumentsDistinct()
        {
            var parametersBuilder = new SqlParametersBuilder();
            var query = SampleInputs.NoArgumentsQuery;
            query.Distinct = true;
            var builder = new SqlQueryBuilder(SampleInputs.Table, query, parametersBuilder, null, null);

            Assert.Throws<InputArgumentException>(builder.Build);
        }

        [Fact]
        public void BuildQueryDistinct()
        {
            var parametersBuilder = new SqlParametersBuilder();
            var query = SampleInputs.NoArgumentsQuery;
            query.Distinct = true;
            query.Columns = ["Category", "SubCategory"];
            var builder = new SqlQueryBuilder(SampleInputs.Table, query, parametersBuilder, null, null);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal(@"SELECT DISTINCT Category, SubCategory FROM Sample", normalizedQueryText);
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
                + " WHERE (Category = @p0 AND SubCategory IS NULL AND Rating >= @p1 AND (Name LIKE @p2 OR Name LIKE @p3))"
                + " GROUP BY Category, SubCategory"
                + " ORDER BY Category ASC, SubCategory ASC"
                + " OFFSET @p4 ROWS FETCH NEXT @p5 ROWS ONLY", normalizedQueryText);

            var reference = new SqlQueryParameter[]
            {
                new ('@', "p0", "ABC", ColumnType.String),
                new ('@', "p1", 5.0, ColumnType.Double),
                new ('@', "p2", "A%", ColumnType.String),
                new ('@', "p3", "%B%", ColumnType.String),
                new ('@', "p4", query.Skip, ColumnType.Integer),
                new ('@', "p5", query.Take, ColumnType.Integer)
            };

            Assert.Equal(reference, parametersBuilder.Parameters);
        }

        [Fact]
        public void BuildQueryAllArgumentsIncludeNone()
        {
            var query = SampleInputs.AllArgumentsQueryRandomizeInclusionAll;

            var mockRandomPrimitives = Substitute.For<IRandomPrimitives>();
            mockRandomPrimitives.GetRandomBoolean().Returns(true);
            var parametersBuilder = new SqlParametersBuilder();
            var builder = new SqlQueryBuilder(SampleInputs.Table, query, parametersBuilder, null, mockRandomPrimitives);

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

            var mockRandomPrimitives = Substitute.For<IRandomPrimitives>();
            mockRandomPrimitives.GetRandomBoolean().Returns(true);
            var parametersBuilder = new SqlParametersBuilder();
            var builder = new SqlQueryBuilder(SampleInputs.Table, query, parametersBuilder, null, mockRandomPrimitives);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal("SELECT Category, SubCategory, SUM(Price) TotalPrice FROM Sample"
                + " WHERE (SubCategory IS NULL AND Rating >= @p0 AND (Name LIKE @p1 OR Name LIKE @p2))"
                + " GROUP BY Category, SubCategory"
                + " ORDER BY Category ASC, SubCategory ASC"
                + " OFFSET @p3 ROWS FETCH NEXT @p4 ROWS ONLY", normalizedQueryText);

            var reference = new SqlQueryParameter[]
            {
                new ('@', "p0", 5.0, ColumnType.Double),
                new ('@', "p1", "A%", ColumnType.String),
                new ('@', "p2", "%B%", ColumnType.String),
                new ('@', "p3", query.Skip, ColumnType.Integer),
                new ('@', "p4", query.Take, ColumnType.Integer),
            };

            Assert.Equal(reference, parametersBuilder.Parameters);
        }
    }
}
