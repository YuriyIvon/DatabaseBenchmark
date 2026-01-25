using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Model;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public class ElasticsearchDistinctValuesProvider : IDistinctValuesProvider
    {
        private readonly ElasticsearchClient _client;

        public ElasticsearchDistinctValuesProvider(ElasticsearchClient client)
        {
            _client = client;
        }

        public object[] GetDistinctValues(string tableName, IValueDefinition column, bool unfoldArray)
        {
            const int maxBuckets = 10000;
            const string bucketName = "distinct";

            var result = _client.SearchAsync<object>(s => s
                .Indices(tableName)
                .Size(0)
                .Aggregations(a => a
                    .Add(bucketName, agg => agg
                        .Terms(t => t
                            .Field(column.Name)
                            .Size(maxBuckets))))).GetAwaiter().GetResult();

            var aggregate = result.Aggregations[bucketName];

            return result.Aggregations[bucketName] switch
            {
                StringTermsAggregate stringTerms => stringTerms.Buckets.Select(b => (object)b.Key.Value).ToArray(),
                LongTermsAggregate longTerms => column.Type == ColumnType.DateTime
                    ? longTerms.Buckets.Select(b => (object)DateTimeOffset.FromUnixTimeMilliseconds(b.Key).UtcDateTime).ToArray()
                    : longTerms.Buckets.Select(b => (object)b.Key).ToArray(),
                DoubleTermsAggregate doubleTerms => doubleTerms.Buckets.Select(b => (object)b.Key).ToArray(),
                _ => throw new InvalidOperationException($"Unsupported aggregate type: {aggregate.GetType()}")
            };
        }
    }
}
