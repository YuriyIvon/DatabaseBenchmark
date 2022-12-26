using DatabaseBenchmark.Databases.Interfaces;
using Nest;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public sealed class ElasticsearchPreparedQuery : IPreparedQuery
    {
        private readonly ElasticClient _client;
        private readonly SearchRequest _request;

        private ElasticsearchQueryResults _results;

        public IDictionary<string, double> CustomMetrics => null;

        public IQueryResults Results => _results;

        public ElasticsearchPreparedQuery(ElasticClient client, SearchRequest request)
        {
            _client = client;
            _request = request;
        }

        public void Execute()
        {
            var response = _client.Search<Dictionary<string, object>>(_request);
            _results = new ElasticsearchQueryResults(response);
        }

        public void Dispose()
        {
        }
    }
}
