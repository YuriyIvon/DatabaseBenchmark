using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.PostgreSql;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Model;
using DatabaseBenchmark.Tests.Utils;
using MongoDB.Driver.Linq;
using NSubstitute;
using System;
using System.Linq;
using Xunit;

namespace DatabaseBenchmark.Tests.Databases
{
    public class PostgreSqlJsonbQueryBuilderTests
    {
        private readonly IOptionsProvider _optionsProvider;

        public PostgreSqlJsonbQueryBuilderTests()
        {
            _optionsProvider = Substitute.For<IOptionsProvider>();
            _optionsProvider.GetOptions<PostgreSqlJsonbQueryOptions>().Returns(new PostgreSqlJsonbQueryOptions());
        }

        [Fact]
        public void BuildQueryNoArguments()
        {
            var parametersBuilder = new SqlParametersBuilder();
            var builder = new PostgreSqlJsonbQueryBuilder(SampleInputs.Table, SampleInputs.NoArgumentsQuery, parametersBuilder, null, null, _optionsProvider);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal(@"SELECT * FROM Sample", normalizedQueryText);
        }

        [Fact]
        public void BuildQuerySelectSpecificFields()
        {
            var parametersBuilder = new SqlParametersBuilder();
            var builder = new PostgreSqlJsonbQueryBuilder(SampleInputs.Table, SampleInputs.SpecificFieldsQuery, parametersBuilder, null, null, _optionsProvider);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal(@"SELECT (attributes->>'Id')::integer Id, "
                + "attributes->>'Category' Category, "
                + "attributes->>'Name' Name, "
                + "attributes->>'CreatedAt' CreatedAt, "
                + "(attributes->>'Rating')::double Rating, "
                + "Price Price FROM Sample",
                normalizedQueryText);
        }

        [Fact]
        public void BuildQueryAllArgumentsGinOperators()
        {
            var query = SampleInputs.AllArgumentsQuery;
            var parametersBuilder = new SqlParametersBuilder();
            var builder = new PostgreSqlJsonbQueryBuilder(SampleInputs.Table, query, parametersBuilder, null, null, _optionsProvider);

            Assert.Throws<InputArgumentException>(() => builder.Build());
        }

        [Fact]
        public void BuildQueryAllArgumentsGinOperatorsNoStringConditions()
        {
            var query = SampleInputs.AllArgumentsQuery;
            //Exclude string conditions
            var groupConditions = (QueryGroupCondition)query.Condition;
            groupConditions.Conditions = groupConditions.Conditions.Where(c => c is QueryPrimitiveCondition).ToArray();

            var parametersBuilder = new SqlParametersBuilder();
            var builder = new PostgreSqlJsonbQueryBuilder(SampleInputs.Table, query, parametersBuilder, null, null, _optionsProvider);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal("SELECT attributes->>'Category' Category, attributes->>'SubCategory' SubCategory, SUM(Price) TotalPrice FROM Sample"
                + " WHERE (attributes @> '{\"Category\": \"ABC\"}'::jsonb AND attributes @> '{\"SubCategory\": null}'::jsonb AND attributes @@ '$.Rating >= 5')"
                + " GROUP BY attributes->>'Category', attributes->>'SubCategory'"
                + " ORDER BY attributes->>'Category' ASC, attributes->>'SubCategory' ASC"
                + " OFFSET @p0 ROWS FETCH NEXT @p1 ROWS ONLY", normalizedQueryText);

            var reference = new SqlQueryParameter[]
            {
                new ('@', "p0", query.Skip, ColumnType.Integer),
                new ('@', "p1", query.Take, ColumnType.Integer)
            };

            Assert.Equal(reference, parametersBuilder.Parameters);
        }

        [Fact]
        public void BuildQueryAllArgumentsNoGinOperators()
        {
            var query = SampleInputs.AllArgumentsQuery;
            var parametersBuilder = new SqlParametersBuilder();
            _optionsProvider.GetOptions<PostgreSqlJsonbQueryOptions>().Returns(new PostgreSqlJsonbQueryOptions {  UseGinOperators = false });
            var builder = new PostgreSqlJsonbQueryBuilder(SampleInputs.Table, query, parametersBuilder, null, null, _optionsProvider);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal("SELECT attributes->>'Category' Category, attributes->>'SubCategory' SubCategory, SUM(Price) TotalPrice FROM Sample"
                + " WHERE (attributes->>'Category' = @p0 AND attributes->>'SubCategory' IS NULL AND (attributes->>'Rating')::double >= @p1 AND (attributes->>'Name' LIKE @p2 OR attributes->>'Name' LIKE @p3))"
                + " GROUP BY attributes->>'Category', attributes->>'SubCategory'"
                + " ORDER BY attributes->>'Category' ASC, attributes->>'SubCategory' ASC"
                + " OFFSET @p4 ROWS FETCH NEXT @p5 ROWS ONLY", normalizedQueryText);

            var reference = new SqlQueryParameter[]
            {
                new ('@', "p0", "ABC", ColumnType.String),
                new ('@', "p1", 5.0, ColumnType.Double),
                new ('@', "p2", "A%", ColumnType.String),
                new ('@', "p3", "%B%", ColumnType.String),
                new ('@', "p4", query.Skip, ColumnType.Integer),
                new ('@', "p5", query.Take, ColumnType.Integer),
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
            var builder = new PostgreSqlJsonbQueryBuilder(SampleInputs.Table, query, parametersBuilder, null, mockRandomPrimitives, _optionsProvider);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal("SELECT attributes->>'Category' Category, attributes->>'SubCategory' SubCategory, SUM(Price) TotalPrice FROM Sample"
                + " GROUP BY attributes->>'Category', attributes->>'SubCategory'"
                + " ORDER BY attributes->>'Category' ASC, attributes->>'SubCategory' ASC"
                + " OFFSET @p0 ROWS FETCH NEXT @p1 ROWS ONLY", normalizedQueryText);

            var reference = new SqlQueryParameter[]
            {
                new ('@', "p0", query.Skip, ColumnType.Integer),
                new ('@', "p1", query.Take, ColumnType.Integer)
            };

            Assert.Equal(reference, parametersBuilder.Parameters);
        }

        [Fact]
        public void BuildQueryAllArgumentsIncludePartialNoStringConditions()
        {
            var query = SampleInputs.AllArgumentsQueryRandomizeInclusionPartial;
            //Exclude string conditions
            var groupConditions = (QueryGroupCondition)query.Condition;
            groupConditions.Conditions = groupConditions.Conditions.Where(c => c is QueryPrimitiveCondition).ToArray();

            var mockRandomPrimitives = Substitute.For<IRandomPrimitives>();
            mockRandomPrimitives.GetRandomBoolean().Returns(true);
            var parametersBuilder = new SqlParametersBuilder();
            var builder = new PostgreSqlJsonbQueryBuilder(SampleInputs.Table, query, parametersBuilder, null, mockRandomPrimitives, _optionsProvider);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal("SELECT attributes->>'Category' Category, attributes->>'SubCategory' SubCategory, SUM(Price) TotalPrice FROM Sample"
                + " WHERE (attributes @> '{\"SubCategory\": null}'::jsonb AND attributes @@ '$.Rating >= 5')"
                + " GROUP BY attributes->>'Category', attributes->>'SubCategory'"
                + " ORDER BY attributes->>'Category' ASC, attributes->>'SubCategory' ASC"
                + " OFFSET @p0 ROWS FETCH NEXT @p1 ROWS ONLY", normalizedQueryText);

            var reference = new SqlQueryParameter[]
            {
                new ('@', "p0", query.Skip, ColumnType.Integer),
                new ('@', "p1", query.Take, ColumnType.Integer)
            };

            Assert.Equal(reference, parametersBuilder.Parameters);
        }
    }
}
