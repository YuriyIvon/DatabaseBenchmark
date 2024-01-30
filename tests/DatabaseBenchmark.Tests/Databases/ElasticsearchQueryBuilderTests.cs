using DatabaseBenchmark.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
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
        public void BuildQueryNoArgumentsDistinct()
        {
            var query = SampleInputs.NoArgumentsQuery;
            query.Distinct = true;
            var builder = new ElasticsearchQueryBuilder(SampleInputs.Table, query, null, null);

            Assert.Throws<InputArgumentException>(builder.Build);
        }

        [Fact]
        public void BuildQueryDistinct()
        {
            var query = SampleInputs.NoArgumentsQuery;
            query.Distinct = true;
            query.Columns = ["Category", "SubCategory"];
            var builder = new ElasticsearchQueryBuilder(SampleInputs.Table, query, null, null);

            Assert.Throws<InputArgumentException>(builder.Build);
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
                "\"query\":{\"bool\":{\"must\":[{\"term\":{\"Category\":{\"value\":\"ABC\"}}},{\"bool\":{\"must_not\":[{\"exists\":{\"field\":\"SubCategory\"}}]}},{\"range\":{\"Rating\":{\"gte\":5.0}}},{\"bool\":{\"should\":[{\"wildcard\":{\"Name\":{\"value\":\"A*\"}}},{\"wildcard\":{\"Name\":{\"value\":\"*B*\"}}}]}}]}}," +
                "\"size\":100}",
                rawQuery);
        }

        [Fact]
        public void BuildQueryAllArgumentsIncludeNone()
        {
            var query = SampleInputs.AllArgumentsQueryRandomizeInclusionAll;

            var mockRandomPrimitives = Substitute.For<IRandomPrimitives>();
            mockRandomPrimitives.GetRandomBoolean().Returns(true);
            var builder = new ElasticsearchQueryBuilder(SampleInputs.Table, query, null, mockRandomPrimitives);

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

            var mockRandomPrimitives = Substitute.For<IRandomPrimitives>();
            mockRandomPrimitives.GetRandomBoolean().Returns(true);
            var builder = new ElasticsearchQueryBuilder(SampleInputs.Table, query, null, mockRandomPrimitives);

            var request = builder.Build();
            var rawQuery = SerializeSearchRequest(request);

            Assert.Equal("{\"aggs\":{\"grouping\":{\"aggs\":{\"TotalPrice\":{\"sum\":{\"field\":\"Price\"}}},\"composite\":{\"size\":10000,\"sources\":[{\"Category\":{\"terms\":{\"field\":\"Category\",\"order\":\"asc\"}}},{\"SubCategory\":{\"terms\":{\"field\":\"SubCategory\",\"order\":\"asc\"}}}]}}}," +
                "\"fields\":[\"Category\",\"SubCategory\"]," +
                "\"from\":10," +
                "\"query\":{\"bool\":{\"must\":[{\"bool\":{\"must_not\":[{\"exists\":{\"field\":\"SubCategory\"}}]}},{\"range\":{\"Rating\":{\"gte\":5.0}}},{\"bool\":{\"should\":[{\"wildcard\":{\"Name\":{\"value\":\"A*\"}}},{\"wildcard\":{\"Name\":{\"value\":\"*B*\"}}}]}}]}}," +
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
