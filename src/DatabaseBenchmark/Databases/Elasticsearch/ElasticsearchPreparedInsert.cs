using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Model;
using Nest;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public sealed class ElasticsearchPreparedInsert : IPreparedQuery
    {
        private readonly IElasticClient _client;
        private readonly Table _table;
        private readonly IEnumerable<object> _documents;

        public IDictionary<string, double> CustomMetrics => null;

        public IQueryResults Results => null;

        public ElasticsearchPreparedInsert(
            IElasticClient client,
            Table table,
            IEnumerable<object> documents)
        {
            _client = client;
            _table = table;
            _documents = documents;
        }

        public void Execute()
        {
            _client.IndexMany(_documents, _table.Name);
        }

        public void Dispose()
        {
        }
    }
}
