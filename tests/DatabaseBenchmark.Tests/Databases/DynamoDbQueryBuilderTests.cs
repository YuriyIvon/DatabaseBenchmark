﻿using DatabaseBenchmark.Common;
using DatabaseBenchmark.Databases.DynamoDb;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;
using DatabaseBenchmark.Tests.Utils;
using Xunit;

namespace DatabaseBenchmark.Tests.Databases
{
    public class DynamoDbQueryBuilderTests
    {
        [Fact]
        public void BuildQueryNoArguments()
        {
            var parametersBuilder = new SqlParametersBuilder('?', true);
            var builder = new DynamoDbQueryBuilder(SampleInputs.Table, SampleInputs.NoArgumentsQuery, parametersBuilder, null, null);

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
            var builder = new DynamoDbQueryBuilder(SampleInputs.Table, query, parametersBuilder, null, null);

            Assert.Throws<InputArgumentException>(builder.Build);
        }

        [Fact]
        public void BuildQueryDistinct()
        {
            var parametersBuilder = new SqlParametersBuilder();
            var query = SampleInputs.NoArgumentsQuery;
            query.Distinct = true;
            query.Columns = ["Category"];
            var builder = new DynamoDbQueryBuilder(SampleInputs.Table, query, parametersBuilder, null, null);

            Assert.Throws<InputArgumentException>(builder.Build);
        }

        [Fact]
        public void BuildQueryAllArgumentsNoLimit()
        {
            var query = SampleInputs.AllArgumentsQuery;
            query.Take = 0;
            query.Skip = 0;

            var parametersBuilder = new SqlParametersBuilder('?', true);
            var builder = new DynamoDbQueryBuilder(SampleInputs.Table, query, parametersBuilder, null, null);

            Assert.Throws<InputArgumentException>(builder.Build);
        }

        [Fact]
        public void BuildQueryAllArgumentsNoAggregation()
        {
            var query = SampleInputs.AllArgumentsQuery;
            query.Aggregate = null;

            var parametersBuilder = new SqlParametersBuilder('?', true);
            var builder = new DynamoDbQueryBuilder(SampleInputs.Table, query, parametersBuilder, null, null);

            Assert.Throws<InputArgumentException>(builder.Build);
        }

        [Fact]
        public void BuildQueryAllArgumentsNoAggregationNoOffset()
        {
            var query = SampleInputs.AllArgumentsQuery;
            query.Aggregate = null;
            query.Take = 0;
            query.Skip = 0;

            var parametersBuilder = new SqlParametersBuilder('?', true);
            var builder = new DynamoDbQueryBuilder(SampleInputs.Table, query, parametersBuilder, null, null);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal("SELECT Category, SubCategory FROM Sample"
                + " WHERE (Category = ? AND attribute_not_exists(SubCategory) AND Rating >= ? AND (begins_with(Name, ?) OR contains(Name, ?)))"
                + " ORDER BY Category ASC, SubCategory ASC", normalizedQueryText);

            var reference = new SqlQueryParameter[]
            {
                new ('?', "p0", "ABC", ColumnType.String),
                new ('?', "p1", 5.0, ColumnType.Double),
                new ('?', "p2", "A", ColumnType.String),
                new ('?', "p3", "B", ColumnType.String)
            };

            Assert.Equal(reference, parametersBuilder.Parameters);
        }

        [Fact]
        public void BuildQueryArrayColumn()
        {
            var query = SampleInputs.ArrayColumnQuery;

            var parametersBuilder = new SqlParametersBuilder('?', true);
            var builder = new DynamoDbQueryBuilder(SampleInputs.ArrayColumnTable, query, parametersBuilder, null, null);

            var queryText = builder.Build();

            var normalizedQueryText = queryText.NormalizeSpaces();
            Assert.Equal("SELECT Category, SubCategory FROM Sample"
                + " WHERE (contains(Tags, ?) OR Tags = ?)", normalizedQueryText);

            var reference = new SqlQueryParameter[]
            {
                new ('?', "p0", "ABC", ColumnType.String),
                new ('?', "p1", new object[] { "A", "B", "C" }, ColumnType.String, true)
            };

            Assert.Equal(reference, parametersBuilder.Parameters, new SqlQueryParameterEqualityComparer());
        }

        [Theory]
        [InlineData(QueryPrimitiveOperator.In)]
        [InlineData(QueryPrimitiveOperator.Greater)]
        [InlineData(QueryPrimitiveOperator.GreaterEquals)]
        [InlineData(QueryPrimitiveOperator.Lower)]
        [InlineData(QueryPrimitiveOperator.LowerEquals)]
        [InlineData(QueryPrimitiveOperator.StartsWith)]
        public void BuildQueryArrayColumnUnsupportedOperator(QueryPrimitiveOperator @operator)
        {
            var query = SampleInputs.ArrayColumnQuery;
            //TODO: change to a query generator function that returns a query with the specified operator
            ((QueryPrimitiveCondition)((QueryGroupCondition)query.Condition).Conditions[0]).Operator = @operator;

            var parametersBuilder = new SqlParametersBuilder('?', true);
            var builder = new DynamoDbQueryBuilder(SampleInputs.ArrayColumnTable, query, parametersBuilder, null, null);

            Assert.Throws<InputArgumentException>(builder.Build);
        }
    }
}
