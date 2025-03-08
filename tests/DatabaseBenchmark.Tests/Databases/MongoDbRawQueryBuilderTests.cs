using DatabaseBenchmark.Databases.MongoDb;
using DatabaseBenchmark.Tests.Utils;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using Xunit;

namespace DatabaseBenchmark.Tests.Databases
{
    public class MongoDbRawQueryBuilderTests
    {
        [Fact]
        public void BuildParameterizedQuery()
        {
            var query = SampleInputs.RawMongoDbQuery;
            var builder = new MongoDbRawQueryBuilder(query, null);

            var queryParts = builder.Build();

            var queryText = new BsonArray(queryParts).ToString();
            Assert.Equal(@"[{ ""$match"" : { ""$and"" : [{ ""category"" : ""ABC"" }, { ""createdDate"" : { ""$gte"" : ""2020-01-02T03:04:05"" } }, { ""price"" : { ""$lte"" : 25.5 } }, { ""available"" : true }] } }]", queryText);
        }

        [Fact]
        public void BuildInlineParameterizedQuery()
        {
            var query = SampleInputs.RawMongoDbInlineQuery;
            var builder = new MongoDbRawQueryBuilder(query, null);

            var queryParts = builder.Build();

            var queryText = new BsonArray(queryParts).ToString();
            Assert.Equal(@"[{ ""$match"" : { ""$and"" : [{ ""category"" : ""ABC"" }, { ""createdDate"" : { ""$gte"" : ""2020-01-02T03:04:05.0000000"" } }, { ""price"" : { ""$lte"" : 25.5 } }, { ""available"" : true }] } }]", queryText);
        }
    }
}
