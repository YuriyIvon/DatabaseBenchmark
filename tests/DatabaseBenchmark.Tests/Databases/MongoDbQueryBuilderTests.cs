using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.MongoDb;
using DatabaseBenchmark.Model;
using DatabaseBenchmark.Tests.Utils;
using MongoDB.Bson;
using NSubstitute;
using System.Linq;
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

            Assert.Equal("[{ \"$match\" : { \"$and\" : [{ \"Category\" : \"ABC\" }, { \"SubCategory\" : null }] } }," +
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

            var mockRandomValueProvider = Substitute.For<IRandomGenerator>();
            mockRandomValueProvider.GetRandomBoolean().Returns(true);
            var builder = new MongoDbQueryBuilder(SampleInputs.Table, query, null, mockRandomValueProvider);

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

            var mockRandomValueProvider = Substitute.For<IRandomGenerator>();
            mockRandomValueProvider.GetRandomBoolean().Returns(true);
            var builder = new MongoDbQueryBuilder(SampleInputs.Table, query, null, mockRandomValueProvider);

            var queryBson = builder.Build();
            var queryText = queryBson.ToJson();

            Assert.Equal("[{ \"$match\" : { \"$and\" : [{ \"SubCategory\" : null }] } }," +
                " { \"$group\" : { \"_id\" : { \"Category\" : \"$Category\", \"SubCategory\" : \"$SubCategory\" }, \"TotalPrice\" : { \"$sum\" : \"$Price\" } } }," +
                " { \"$sort\" : { \"_id.Category\" : 1, \"_id.SubCategory\" : 1 } }," +
                " { \"$skip\" : 10 }, { \"$limit\" : 100 }," +
                " { \"$project\" : { \"_id\" : 0, \"Category\" : \"$_id.Category\", \"SubCategory\" : \"$_id.SubCategory\", \"TotalPrice\" : \"$TotalPrice\" } }]",
                queryText);
        }
    }
}
