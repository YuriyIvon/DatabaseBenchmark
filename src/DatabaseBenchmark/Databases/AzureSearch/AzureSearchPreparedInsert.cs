using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using DatabaseBenchmark.Databases.Common.Interfaces;

namespace DatabaseBenchmark.Databases.AzureSearch
{
    public sealed class AzureSearchPreparedInsert : IPreparedQuery
    {
        private readonly SearchClient _client;
        private readonly IEnumerable<object> _documents;

        public IDictionary<string, double> CustomMetrics => null;

        public IQueryResults Results => null;

        public AzureSearchPreparedInsert(
            SearchClient client,
            IEnumerable<object> documents)
        {
            _client = client;
            _documents = documents;
        }

        public int Execute()
        {
            var response = _client.IndexDocuments(IndexDocumentsBatch.Upload(_documents));
                       
            return response.Value.Results.Count;
        }

        public void Dispose()
        {
        }
    }
}