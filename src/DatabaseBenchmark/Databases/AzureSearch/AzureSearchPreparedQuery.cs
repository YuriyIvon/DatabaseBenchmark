using Azure.Search.Documents;
using DatabaseBenchmark.Databases.Common.Interfaces;

namespace DatabaseBenchmark.Databases.AzureSearch
{
    public sealed class AzureSearchPreparedQuery : IPreparedQuery
    {
        private readonly SearchClient _client;
        private readonly SearchOptions _options;

        private AzureSearchQueryResults _results;

        public IDictionary<string, double> CustomMetrics => null;

        public IQueryResults Results => _results;

        public AzureSearchPreparedQuery(SearchClient client, SearchOptions options)
        {
            _client = client;
            _options = options;
        }

        public int Execute()
        {
            var response = _client.Search<Dictionary<string, object>>(_options);
            _results = new AzureSearchQueryResults(response.Value);

            return response.Value.GetResults().Count();
        }

        public void Dispose()
        {
        }
    }
}