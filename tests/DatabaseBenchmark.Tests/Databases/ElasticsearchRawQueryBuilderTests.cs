using DatabaseBenchmark.Databases.Elasticsearch;
using DatabaseBenchmark.Tests.Utils;
using Nest;
using System.IO;
using Xunit;

namespace DatabaseBenchmark.Tests.Databases
{
    public class ElasticsearchRawQueryBuilderTests
    {
        [Fact]
        public void BuildParameterizedQuery()
        {
            var query = SampleInputs.RawElasticsearchQuery;
            //TODO: Find a better way to instantiate the default serializer
            var serializer = new ElasticClient().RequestResponseSerializer;
            var builder = new ElasticsearchRawQueryBuilder(query, serializer, null);

            var searchRequest = builder.Build();

            using var stream = new MemoryStream();
            serializer.Serialize(searchRequest, stream);
            var queryText = stream.ReadAsString();

            Assert.Equal(@"{""query"":{""bool"":{""must"":[{""term"":{""category"":{""value"":""ABC""}}},{""range"":{""createdDate"":{""gte"":""2020-01-02T03:04:05""}}},{""range"":{""price"":{""lte"":25.5}}},{""term"":{""available"":{""value"":true}}}]}}}", queryText);
        }

        [Fact]
        public void BuildInlineParameterizedQuery()
        {
            var query = SampleInputs.RawElasticsearchInlineQuery;
            //TODO: Find a better way to instantiate the default serializer
            var serializer = new ElasticClient().RequestResponseSerializer;
            var builder = new ElasticsearchRawQueryBuilder(query, serializer, null);

            var searchRequest = builder.Build();

            using var stream = new MemoryStream();
            serializer.Serialize(searchRequest, stream);
            var queryText = stream.ReadAsString();

            Assert.Equal(@"{""query"":{""bool"":{""must"":[{""term"":{""category"":{""value"":""ABC""}}},{""range"":{""createdDate"":{""gte"":""2020-01-02T03:04:05""}}},{""range"":{""price"":{""lte"":25.5}}},{""term"":{""available"":{""value"":true}}}]}}}", queryText);
        }
    }
}
