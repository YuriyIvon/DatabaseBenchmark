using DatabaseBenchmark.Databases.Common.Interfaces;
using Elastic.Clients.Elasticsearch;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public sealed class ElasticsearchPreparedQuery : IPreparedQuery
    {
        private readonly ElasticsearchClient _client;
        private readonly SearchRequest _request;

        private ElasticsearchQueryResults _results;

        public IDictionary<string, double> CustomMetrics => null;

        public IQueryResults Results => _results;

        public ElasticsearchPreparedQuery(ElasticsearchClient client, SearchRequest request)
        {
            _client = client;
            _request = request;
        }

        public int Execute()
        {
            var response = _client.SearchAsync<Dictionary<string, object>>(_request).GetAwaiter().GetResult();
            _results = new ElasticsearchQueryResults(response);

            return 0;
        }

        public void Dispose()
        {
        }
    }
}
