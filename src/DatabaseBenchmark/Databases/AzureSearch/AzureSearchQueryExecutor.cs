using Azure.Search.Documents;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.AzureSearch.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using System.Text.Json;

namespace DatabaseBenchmark.Databases.AzureSearch
{
    public sealed class AzureSearchQueryExecutor : IQueryExecutor
    {
        private readonly SearchClient _client;
        private readonly IAzureSearchQueryBuilder _queryBuilder;
        private readonly IExecutionEnvironment _environment;
        private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

        public AzureSearchQueryExecutor(
            SearchClient client,
            IAzureSearchQueryBuilder queryBuilder,
            IExecutionEnvironment environment)
        {
            _client = client;
            _queryBuilder = queryBuilder;
            _environment = environment;
        }

        public IPreparedQuery Prepare()
        {
            var options = _queryBuilder.Build();

            if (_environment.TraceQueries)
            {
                _environment.WriteLine("Query:");
                _environment.WriteLine(JsonSerializer.Serialize(options, _serializerOptions));
                _environment.WriteLine(string.Empty);
            }

            return new AzureSearchPreparedQuery(_client, options);
        }

        public IPreparedQuery Prepare(ITransaction transaction) => Prepare();

        public void Dispose()
        {
        }
    }
}