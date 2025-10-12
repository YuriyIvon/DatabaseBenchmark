using Azure.Search.Documents;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.AzureSearch
{
    public class AzureSearchDistinctValuesProvider : IDistinctValuesProvider
    {
        private readonly SearchClient _client;

        public AzureSearchDistinctValuesProvider(SearchClient client)
        {
            _client = client;
        }

        public object[] GetDistinctValues(string tableName, IValueDefinition column, bool unfoldArray)
        {
            const int maxCount = 10000; // Azure Search has a limit for facet counts

            var options = new SearchOptions
            {
                Size = 0,
                Facets = { $"{column.Name},count:{maxCount}" }
            };

            var facetResults = _client.Search<Dictionary<string, object>>("*", options);
            
            if (facetResults.Value.Facets.TryGetValue(column.Name, out var facetResult))
            {
                return facetResult.Select(f => f.Value).ToArray();
            }
            
            return [];
        }
    }
}