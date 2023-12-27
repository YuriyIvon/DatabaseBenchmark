﻿using DatabaseBenchmark.Databases.MongoDb;
using DatabaseBenchmark.Generators.Interfaces;
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
        public void BuildQueryAllArguments()
        {
            var query = SampleInputs.AllArgumentsQuery;
            var builder = new MongoDbQueryBuilder(SampleInputs.Table, query, null, null);

            var queryBson = builder.Build();
            var queryText = queryBson.ToJson();

            Assert.Equal("[{ \"$match\" : { \"$and\" : [{ \"Category\" : \"ABC\" }, { \"SubCategory\" : null }, { \"Rating\" : { \"$gte\" : 5.0 } }, { \"$or\" : [{ \"Name\" : { \"$regex\" : \"^A\" } }, { \"Name\" : { \"$regex\" : \"B\" } }] }] } }," +
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

            Assert.Equal("[{ \"$match\" : { \"$and\" : [{ \"SubCategory\" : null }, { \"Rating\" : { \"$gte\" : 5.0 } }, { \"$or\" : [{ \"Name\" : { \"$regex\" : \"^A\" } }, { \"Name\" : { \"$regex\" : \"B\" } }] }] } }," +
                " { \"$group\" : { \"_id\" : { \"Category\" : \"$Category\", \"SubCategory\" : \"$SubCategory\" }, \"TotalPrice\" : { \"$sum\" : \"$Price\" } } }," +
                " { \"$sort\" : { \"_id.Category\" : 1, \"_id.SubCategory\" : 1 } }," +
                " { \"$skip\" : 10 }, { \"$limit\" : 100 }," +
                " { \"$project\" : { \"_id\" : 0, \"Category\" : \"$_id.Category\", \"SubCategory\" : \"$_id.SubCategory\", \"TotalPrice\" : \"$TotalPrice\" } }]",
                queryText);
        }
    }
}
