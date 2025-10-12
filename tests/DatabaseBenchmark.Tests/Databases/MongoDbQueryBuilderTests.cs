using DatabaseBenchmark.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.MongoDb;
using DatabaseBenchmark.Model;
using DatabaseBenchmark.Tests.Utils;
using MongoDB.Bson;
using NSubstitute;
using Xunit;

namespace DatabaseBenchmark.Tests.Databases
{
    public class MongoDbQueryBuilderTests
    {
        [Fact]
        public void BuildQueryNoArguments()
        {
            var builder = new MongoDbQueryBuilder(SampleInputs.Table, SampleInputs.NoArgumentsQuery, null, null);

            var queryBson = builder.Build();
            var queryText = queryBson.ToJson();

            Assert.Equal("[]", queryText);
        }

        [Fact]
        public void BuildQueryNoArgumentsDistinct()
        {
            var query = SampleInputs.NoArgumentsQuery;
            query.Distinct = true;
            var builder = new MongoDbQueryBuilder(SampleInputs.Table, query, null, null);

            Assert.Throws<InputArgumentException>(builder.Build);
        }

        [Fact]
        public void BuildQueryDistinct()
        {
            var query = SampleInputs.NoArgumentsQuery;
            query.Distinct = true;
            query.Columns = ["Category", "SubCategory"];
            var builder = new MongoDbQueryBuilder(SampleInputs.Table, query, null, null);

            var queryBson = builder.Build();
            var queryText = queryBson.ToJson();

            Assert.Equal("[{ \"$project\" : { \"_id\" : 0, \"Category\" : \"$Category\", \"SubCategory\" : \"$SubCategory\" } }," +
                " { \"$group\" : { \"_id\" : { \"Category\" : \"$Category\", \"SubCategory\" : \"$SubCategory\" } } }," +
                " { \"$project\" : { \"_id\" : 0, \"Category\" : \"$_id.Category\", \"SubCategory\" : \"$_id.SubCategory\" } }]",
                queryText);
        }

        [Fact]
        public void BuildQueryAllArguments()
        {
            var query = SampleInputs.AllArgumentsQuery;
            var builder = new MongoDbQueryBuilder(SampleInputs.Table, query, null, null);

            var queryBson = builder.Build();
            var queryText = queryBson.ToJson();

            Assert.Equal("[{ \"$match\" : { \"$and\" : [{ \"Category\" : { \"$in\" : [\"ABC\", \"DEF\"] } }, { \"SubCategory\" : null }, { \"Rating\" : { \"$gte\" : 5.0 } }, { \"Count\" : 0 }, { \"$or\" : [{ \"Name\" : { \"$regex\" : \"^A\" } }, { \"Name\" : { \"$regex\" : \"B\" } }] }] } }," +
                " { \"$group\" : { \"_id\" : { \"Category\" : \"$Category\", \"SubCategory\" : \"$SubCategory\" }, \"TotalPrice\" : { \"$sum\" : \"$Price\" } } }," +
                " { \"$sort\" : { \"_id.Category\" : 1, \"_id.SubCategory\" : 1 } }," +
                " { \"$skip\" : 10 }, { \"$limit\" : 100 }," +
                " { \"$project\" : { \"_id\" : 0, \"Category\" : \"$_id.Category\", \"SubCategory\" : \"$_id.SubCategory\", \"TotalPrice\" : \"$TotalPrice\" } }]", 
                queryText);
        }

        [Fact]
        public void BuildQueryAllArgumentsIncludeNone()
        {
            var query = SampleInputs.AllArgumentsQueryRandomizeInclusionAll;

            var mockRandomPrimitives = Substitute.For<IRandomPrimitives>();
            mockRandomPrimitives.GetRandomBoolean().Returns(true);
            var builder = new MongoDbQueryBuilder(SampleInputs.Table, query, null, mockRandomPrimitives);

            var queryBson = builder.Build();
            var queryText = queryBson.ToJson();

            Assert.Equal("[{ \"$group\" : { \"_id\" : { \"Category\" : \"$Category\", \"SubCategory\" : \"$SubCategory\" }, \"TotalPrice\" : { \"$sum\" : \"$Price\" } } }," +
                " { \"$sort\" : { \"_id.Category\" : 1, \"_id.SubCategory\" : 1 } }," +
                " { \"$skip\" : 10 }, { \"$limit\" : 100 }," +
                " { \"$project\" : { \"_id\" : 0, \"Category\" : \"$_id.Category\", \"SubCategory\" : \"$_id.SubCategory\", \"TotalPrice\" : \"$TotalPrice\" } }]",
                queryText);
        }

        [Fact]
        public void BuildQueryAllArgumentsIncludePartial()
        {
            var query = SampleInputs.AllArgumentsQueryRandomizeInclusionPartial;

            var mockRandomPrimitives = Substitute.For<IRandomPrimitives>();
            mockRandomPrimitives.GetRandomBoolean().Returns(true);
            var builder = new MongoDbQueryBuilder(SampleInputs.Table, query, null, mockRandomPrimitives);

            var queryBson = builder.Build();
            var queryText = queryBson.ToJson();

            Assert.Equal("[{ \"$match\" : { \"$and\" : [{ \"SubCategory\" : null }, { \"Rating\" : { \"$gte\" : 5.0 } }, { \"Count\" : 0 }, { \"$or\" : [{ \"Name\" : { \"$regex\" : \"^A\" } }, { \"Name\" : { \"$regex\" : \"B\" } }] }] } }," +
                " { \"$group\" : { \"_id\" : { \"Category\" : \"$Category\", \"SubCategory\" : \"$SubCategory\" }, \"TotalPrice\" : { \"$sum\" : \"$Price\" } } }," +
                " { \"$sort\" : { \"_id.Category\" : 1, \"_id.SubCategory\" : 1 } }," +
                " { \"$skip\" : 10 }, { \"$limit\" : 100 }," +
                " { \"$project\" : { \"_id\" : 0, \"Category\" : \"$_id.Category\", \"SubCategory\" : \"$_id.SubCategory\", \"TotalPrice\" : \"$TotalPrice\" } }]",
                queryText);
        }

        [Fact]
        public void BuildQueryArrayColumn()
        {
            var query = SampleInputs.ArrayColumnQuery;

            var builder = new MongoDbQueryBuilder(SampleInputs.ArrayColumnTable, query, null, null);

            var queryBson = builder.Build();
            var queryText = queryBson.ToJson();

            Assert.Equal("[{ \"$match\" : { \"$or\" : [{ \"Tags\" : \"ABC\" }, { \"Tags\" : [\"A\", \"B\", \"C\"] }] } }," +
                " { \"$project\" : { \"_id\" : 0, \"Category\" : \"$Category\", \"SubCategory\" : \"$SubCategory\" } }]",
                queryText);
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

            var builder = new MongoDbQueryBuilder(SampleInputs.ArrayColumnTable, query, null, null);

            Assert.Throws<InputArgumentException>(builder.Build);
        }
    }
}
