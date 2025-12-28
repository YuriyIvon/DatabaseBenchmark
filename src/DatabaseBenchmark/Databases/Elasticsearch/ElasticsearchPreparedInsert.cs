using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Model;
using Elastic.Clients.Elasticsearch;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public sealed class ElasticsearchPreparedInsert : IPreparedQuery
    {
        private readonly ElasticsearchClient _client;
        private readonly Table _table;
        private readonly IEnumerable<object> _documents;

        public IDictionary<string, double> CustomMetrics => null;

        public IQueryResults Results => null;

        public ElasticsearchPreparedInsert(
            ElasticsearchClient client,
            Table table,
            IEnumerable<object> documents)
        {
            _client = client;
            _table = table;
            _documents = documents;
        }

        public int Execute()
        {
            //TODO: make refresh parameter configurable
            var response = _client.BulkAsync(b => b
                .Index(_table.Name.ToLower())
                .IndexMany(_documents)).GetAwaiter().GetResult();

            return response.Items.Count;
        }

        public void Dispose()
        {
        }
    }
}
