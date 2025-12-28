using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Model;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public class ElasticsearchDataMetricsProvider : IDataMetricsProvider
    {
        private readonly ElasticsearchClient _client;
        private readonly Table _table;

        private IndicesStatsResponse _stats;

        public ElasticsearchDataMetricsProvider(ElasticsearchClient client, Table table)
        {
            _client = client;
            _table = table;
        }

        public long GetRowCount()
        {
            EnsureStats();
            return _stats.Indices[_table.Name].Primaries.Docs.Count;
        }

        public IDictionary<string, double> GetMetrics()
        {
            EnsureStats();
            return new Dictionary<string, double>
            {
                [Common.Metrics.TotalStorageBytes] = (double)_stats.Indices[_table.Name].Total.Store.SizeInBytes
            };
        }

        private void EnsureStats()
        {
            _client.Indices.RefreshAsync(_table.Name).GetAwaiter().GetResult();
            _stats ??= _client.Indices.StatsAsync(s => s.Indices(_table.Name)).GetAwaiter().GetResult();
        }
    }
}
