using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Model;
using Nest;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public class ElasticsearchDataMetricsProvider : IDataMetricsProvider
    {
        private readonly IElasticClient _client;
        private readonly Table _table;

        private IndicesStatsResponse _stats;

        public ElasticsearchDataMetricsProvider(IElasticClient client, Table table)
        {
            _client = client;
            _table = table;
        }

        public long GetRowCount()
        {
            EnsureStats();
            return _stats.Stats.Primaries.Documents.Count;
        }

        public IDictionary<string, double> GetMetrics()
        {
            EnsureStats();
            return new Dictionary<string, double>
            {
                [Common.Metrics.TotalStorageBytes] = _stats.Stats.Total.Store.SizeInBytes
            };
        }

        private void EnsureStats()
        {
            _client.Indices.Refresh(_table.Name);
            _stats ??= _client.Indices.Stats(_table.Name);
        }
    }
}
