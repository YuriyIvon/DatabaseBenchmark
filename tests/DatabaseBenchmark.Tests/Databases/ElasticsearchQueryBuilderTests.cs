using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Elasticsearch;
using DatabaseBenchmark.Tests.Utils;
using Nest;
using NSubstitute;
using System.IO;
using Xunit;

namespace DatabaseBenchmark.Tests.Databases
{
    public class ElasticsearchQueryBuilderTests
    {
        [Fact]
        public void BuildQueryNoArguments()
        {
            var builder = new ElasticsearchQueryBuilder(SampleInputs.Table, SampleInputs.NoArgumentsQuery, null, null);

            var request = builder.Build();
            var rawQuery = SerializeSearchRequest(request);

            Assert.Equal("{}", rawQuery);
        }

        [Fact]
        public void BuildQueryAllArguments()
        {
            var query = SampleInputs.AllArgumentsQuery;
            var builder = new ElasticsearchQueryBuilder(SampleInputs.Table, query, null, null);

            var request = builder.Build();
            var rawQuery = SerializeSearchRequest(request);

            Assert.Equal("{\"aggs\":{\"grouping\":{\"aggs\":{\"TotalPrice\":{\"sum\":{\"field\":\"Price\"}}},\"composite\":{\"size\":10000,\"sources\":[{\"Category\":{\"terms\":{\"field\":\"Category\",\"order\":\"asc\"}}},{\"SubCategory\":{\"terms\":{\"field\":\"SubCategory\",\"order\":\"asc\"}}}]}}}," +
                "\"fields\":[\"Category\",\"SubCategory\"]," +
                "\"from\":10," +
                "\"query\":{\"bool\":{\"must\":[{\"term\":{\"Category\":{\"value\":\"ABC\"}}},{\"bool\":{\"must_not\":[{\"exists\":{\"field\":\"SubCategory\"}}]}}]}}," +
                "\"size\":100}",
                rawQuery);
        }

        [Fact]
        public void BuildQueryAllArgumentsIncludeNone()
        {
            var query = SampleInputs.AllArgumentsQueryRandomizeInclusionAll;

            var mockRandomValueProvider = Substitute.For<IRandomGenerator>();
            mockRandomValueProvider.GetRandomBoolean().Returns(true);
            var builder = new ElasticsearchQueryBuilder(SampleInputs.Table, query, null, mockRandomValueProvider);

            var request = builder.Build();
            var rawQuery = SerializeSearchRequest(request);

            Assert.Equal("{\"aggs\":{\"grouping\":{\"aggs\":{\"TotalPrice\":{\"sum\":{\"field\":\"Price\"}}},\"composite\":{\"size\":10000,\"sources\":[{\"Category\":{\"terms\":{\"field\":\"Category\",\"order\":\"asc\"}}},{\"SubCategory\":{\"terms\":{\"field\":\"SubCategory\",\"order\":\"asc\"}}}]}}}," +
                "\"fields\":[\"Category\",\"SubCategory\"]," +
                "\"from\":10," +
                "\"size\":100}",
                rawQuery);
        }

        [Fact]
        public void BuildQueryAllArgumentsIncludePartial()
        {
            var query = SampleInputs.AllArgumentsQueryRandomizeInclusionPartial;

            var mockRandomValueProvider = Substitute.For<IRandomGenerator>();
            mockRandomValueProvider.GetRandomBoolean().Returns(true);
            var builder = new ElasticsearchQueryBuilder(SampleInputs.Table, query, null, mockRandomValueProvider);

            var request = builder.Build();
            var rawQuery = SerializeSearchRequest(request);

            Assert.Equal("{\"aggs\":{\"grouping\":{\"aggs\":{\"TotalPrice\":{\"sum\":{\"field\":\"Price\"}}},\"composite\":{\"size\":10000,\"sources\":[{\"Category\":{\"terms\":{\"field\":\"Category\",\"order\":\"asc\"}}},{\"SubCategory\":{\"terms\":{\"field\":\"SubCategory\",\"order\":\"asc\"}}}]}}}," +
                "\"fields\":[\"Category\",\"SubCategory\"]," +
                "\"from\":10," +
                "\"query\":{\"bool\":{\"must\":[{\"bool\":{\"must_not\":[{\"exists\":{\"field\":\"SubCategory\"}}]}}]}}," +
                "\"size\":100}",
                rawQuery);
        }

        private static string SerializeSearchRequest(SearchRequest searchRequest)
        {
            var elasticClient = new ElasticClient();
            using var stream = new MemoryStream();
            elasticClient.RequestResponseSerializer.Serialize(searchRequest, stream);
            stream.Position = 0;
            using var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }
    }
}
