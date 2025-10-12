using Azure.Search.Documents;
using DatabaseBenchmark.Databases.Common.Interfaces;

namespace DatabaseBenchmark.Databases.AzureSearch
{
    public class AzureSearchDataMetricsProvider : IDataMetricsProvider
    {
        private readonly SearchClient _searchClient;

        public AzureSearchDataMetricsProvider(SearchClient searchClient)
        {
            _searchClient = searchClient;
        }

        public long GetRowCount()
        {
            var options = new SearchOptions
            {
                Size = 0,
                IncludeTotalCount = true
            };

            var response = _searchClient.Search<Dictionary<string, object>>("*", options);
            return (long)response.Value.TotalCount;
        }

        public IDictionary<string, double> GetMetrics() => null;
    }
}