using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Model;
using Nest;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public class ElasticsearchDistinctValuesProvider : IDistinctValuesProvider
    {
        private readonly ElasticClient _client;

        public ElasticsearchDistinctValuesProvider(ElasticClient client)
        {
            _client = client;
        }

        public object[] GetDistinctValues(string tableName, IValueDefinition column, bool unfoldArray)
        {
            const int maxBuckets = 10000;
            const string bucketName = "distinct";

            var result = _client.Search<object>(sd => sd
                .Index(tableName)
                .Size(0)
                .Aggregations(a => a
                    .Terms(bucketName, td => td
                        .Field(column.Name)
                        .Size(maxBuckets))));

            return result.Aggregations
                .Terms<object>(bucketName)
                .Buckets
                .Select(i => i.Key)
                .ToArray();
        }
    }
}
