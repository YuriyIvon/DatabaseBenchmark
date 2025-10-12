using Azure.Search.Documents;

namespace DatabaseBenchmark.Databases.AzureSearch.Interfaces
{
    public interface IAzureSearchQueryBuilder
    {
        SearchOptions Build();
    }
}