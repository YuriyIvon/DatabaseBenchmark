using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Model;
using Nest;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public class ElasticsearchDataMetricsProvider : IDataMetricsProvider
    {
        private readonly IElasticClient _client;
        private readonly Table _table;

        public ElasticsearchDataMetricsProvider(IElasticClient client, Table table)
        {
            _client = client;
            _table = table;
        }

        public long GetRowCount()
        {
            var stats = _client.Indices.Stats(_table.Name);
            return stats.Stats.Primaries.Documents.Count;
        }
    }
}
