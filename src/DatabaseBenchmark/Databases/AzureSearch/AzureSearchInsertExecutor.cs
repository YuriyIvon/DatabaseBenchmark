using Azure.Search.Documents;
using DatabaseBenchmark.Databases.AzureSearch.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;

namespace DatabaseBenchmark.Databases.AzureSearch
{
    public sealed class AzureSearchInsertExecutor : IQueryExecutor
    {
        private readonly SearchClient _client;
        private readonly IAzureSearchInsertBuilder _insertBuilder;

        public AzureSearchInsertExecutor(
            SearchClient client,
            IAzureSearchInsertBuilder insertBuilder)
        {
            _client = client;
            _insertBuilder = insertBuilder;
        }

        public IPreparedQuery Prepare()
        {
            var documents = _insertBuilder.Build();

            return new AzureSearchPreparedInsert(_client, documents);
        }

        public IPreparedQuery Prepare(ITransaction transaction) => Prepare();

        public void Dispose()
        {
        }
    }
}