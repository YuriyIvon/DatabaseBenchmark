﻿using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.PostgreSql;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Model;
using DatabaseBenchmark.Tests.Utils;
using NSubstitute;
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
            var parametersBuilder = new SqlQueryParametersBuilder();
            var builder = new PostgreSqlJsonbQueryBuilder(SampleInputs.Table, SampleInputs.NoArgumentsQuery, parametersBuilder, null, null, _optionsProvider);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal(@"SELECT * FROM Sample", normalizedQueryText);
        }

        [Fact]
        public void BuildQuerySelectSpecificFields()
        {
            var parametersBuilder = new SqlQueryParametersBuilder();
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
            var parametersBuilder = new SqlQueryParametersBuilder();
            var builder = new PostgreSqlJsonbQueryBuilder(SampleInputs.Table, query, parametersBuilder, null, null, _optionsProvider);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal("SELECT attributes->>'Category' Category, attributes->>'SubCategory' SubCategory, SUM(Price) TotalPrice FROM Sample"
                + " WHERE (attributes @> '{\"Category\": \"ABC\"}'::jsonb AND attributes @> '{\"SubCategory\": null}'::jsonb)"
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
            var parametersBuilder = new SqlQueryParametersBuilder();
            _optionsProvider.GetOptions<PostgreSqlJsonbQueryOptions>().Returns(new PostgreSqlJsonbQueryOptions {  UseGinOperators = false });
            var builder = new PostgreSqlJsonbQueryBuilder(SampleInputs.Table, query, parametersBuilder, null, null, _optionsProvider);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal("SELECT attributes->>'Category' Category, attributes->>'SubCategory' SubCategory, SUM(Price) TotalPrice FROM Sample"
                + " WHERE (attributes->>'Category' = @p0 AND attributes->>'SubCategory' IS NULL)"
                + " GROUP BY attributes->>'Category', attributes->>'SubCategory'"
                + " ORDER BY attributes->>'Category' ASC, attributes->>'SubCategory' ASC"
                + " OFFSET @p1 ROWS FETCH NEXT @p2 ROWS ONLY", normalizedQueryText);

            var reference = new SqlQueryParameter[]
            {
                new ('@', "p0", "ABC", ColumnType.String),
                new ('@', "p1", query.Skip, ColumnType.Integer),
                new ('@', "p2", query.Take, ColumnType.Integer)
            };

            Assert.Equal(reference, parametersBuilder.Parameters);
        }

        [Fact]
        public void BuildQueryAllArgumentsIncludeNone()
        {
            var query = SampleInputs.AllArgumentsQueryRandomizeInclusionAll;

            var mockRandomValueProvider = Substitute.For<IRandomGenerator>();
            mockRandomValueProvider.GetRandomBoolean().Returns(true);
            var parametersBuilder = new SqlQueryParametersBuilder();
            var builder = new PostgreSqlJsonbQueryBuilder(SampleInputs.Table, query, parametersBuilder, null, mockRandomValueProvider, _optionsProvider);

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
        public void BuildQueryAllArgumentsIncludePartial()
        {
            var query = SampleInputs.AllArgumentsQueryRandomizeInclusionPartial;

            var mockRandomValueProvider = Substitute.For<IRandomGenerator>();
            mockRandomValueProvider.GetRandomBoolean().Returns(true);
            var parametersBuilder = new SqlQueryParametersBuilder();
            var builder = new PostgreSqlJsonbQueryBuilder(SampleInputs.Table, query, parametersBuilder, null, mockRandomValueProvider, _optionsProvider);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal("SELECT attributes->>'Category' Category, attributes->>'SubCategory' SubCategory, SUM(Price) TotalPrice FROM Sample"
                + " WHERE (attributes @> '{\"SubCategory\": null}'::jsonb)"
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
