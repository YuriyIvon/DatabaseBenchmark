using DatabaseBenchmark.Databases.MongoDb;
using DatabaseBenchmark.Tests.Utils;
using MongoDB.Bson;
using Xunit;

namespace DatabaseBenchmark.Tests.Databases
{
    public class MongoDbQueryBuilderTests
    {
        [Fact]
        public void BuildQueryNoArguments()
        {
            var builder = new MongoDbQueryBuilder(SampleInputs.Table, SampleInputs.NoArgumentsQuery, null);

            var queryBson = builder.Build();
            var queryText = queryBson.ToJson();

            Assert.Equal("[]", queryText);
        }

        [Fact]
        public void BuildQueryAllArguments()
        {
            var query = SampleInputs.AllArgumentsQuery;
            var builder = new MongoDbQueryBuilder(SampleInputs.Table, query, null);

            var queryBson = builder.Build();
            var queryText = queryBson.ToJson();

            Assert.Equal("[{ \"$match\" : { \"$and\" : [{ \"Category\" : \"ABC\" }, { \"SubCategory\" : null }] } },"+
                " { \"$group\" : { \"_id\" : { \"Category\" : \"$Category\", \"SubCategory\" : \"$SubCategory\" }, \"TotalPrice\" : { \"$sum\" : \"$Price\" } } }," +
                " { \"$sort\" : { \"_id.Category\" : 1, \"_id.SubCategory\" : 1 } }," +
                " { \"$skip\" : 10 }, { \"$limit\" : 100 }," +
                " { \"$project\" : { \"_id\" : 0, \"Category\" : \"$_id.Category\", \"SubCategory\" : \"$_id.SubCategory\", \"TotalPrice\" : \"$TotalPrice\" } }]", 
                queryText);
        }
    }
}
