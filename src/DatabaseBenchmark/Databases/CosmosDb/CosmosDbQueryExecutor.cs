using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using Microsoft.Azure.Cosmos;
using System.Text;

namespace DatabaseBenchmark.Databases.CosmosDb
{
    public sealed class CosmosDbQueryExecutor : IQueryExecutor
    {
        private readonly CosmosClient _client;
        private readonly Container _container;
        private readonly ISqlParametersBuilder _parametersBuilder;
        private readonly ISqlQueryBuilder _queryBuilder;
        private readonly IExecutionEnvironment _environment;
        private readonly IOptionsProvider _optionsProvider;

        public CosmosDbQueryExecutor(
            CosmosClient client,
            Container container,
            ISqlQueryBuilder queryBuilder,
            ISqlParametersBuilder parametersBuilder,
            IExecutionEnvironment environment,
            IOptionsProvider optionsProvider)
        {
            _client = client;
            _container = container;
            _queryBuilder = queryBuilder;
            _parametersBuilder = parametersBuilder;
            _environment = environment;
            _optionsProvider = optionsProvider;
        }

        public IPreparedQuery Prepare()
        {
            var query = _queryBuilder.Build();

            _environment.TraceCommand(query, _parametersBuilder.Parameters);

            var queryDefinition = new QueryDefinition(query);
            foreach (var parameter in _parametersBuilder.Parameters)
            {
                queryDefinition.WithParameter(parameter.Prefix + parameter.Name, parameter.Value);
            }

            var options = _optionsProvider.GetOptions<CosmosDbQueryOptions>();
            return new CosmosDbPreparedQuery(_container, queryDefinition, options);
        }

        public IPreparedQuery Prepare(ITransaction transaction) => Prepare();

        public void Dispose() => _client?.Dispose();
    }
}
