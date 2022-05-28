using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using Microsoft.Azure.Cosmos;
using System.Text;

namespace DatabaseBenchmark.Databases.CosmosDb
{
    public class CosmosDbQueryExecutor : IQueryExecutor
    {
        private readonly CosmosClient _client;
        private readonly Container _container;
        private readonly SqlParametersBuilder _parametersBuilder;
        private readonly ISqlQueryBuilder _queryBuilder;
        private readonly IExecutionEnvironment _environment;
        private readonly IOptionsProvider _optionsProvider;

        public CosmosDbQueryExecutor(
            CosmosClient client,
            Container container,
            ISqlQueryBuilder queryBuilder,
            SqlParametersBuilder parametersBuilder,
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

            TraceCommand(query, _parametersBuilder.Values);

            var queryDefinition = new QueryDefinition(query);
            foreach (var parameter in _parametersBuilder.Values)
            {
                queryDefinition.WithParameter(parameter.Key, parameter.Value);
            }

            var options = _optionsProvider.GetOptions<CosmosDbQueryOptions>();
            return new CosmosDbPreparedQuery(_container, queryDefinition, options);
        }

        public void Dispose()
        {
            if (_client != null)
            {
                _client.Dispose();
            }
        }

        public void TraceCommand(string query, IDictionary<string, object> parameters)
        {
            if (_environment.TraceQueries)
            {
                var traceBuilder = new StringBuilder();

                traceBuilder.AppendLine("Query:");
                traceBuilder.AppendLine(query);

                if (parameters.Count > 0)
                {
                    traceBuilder.AppendLine("Parameters:");

                    foreach (var parameter in parameters)
                    {
                        traceBuilder.AppendLine($"{parameter.Key}={parameter.Value}");
                    }
                }

                _environment.WriteLine(traceBuilder.ToString());
            }
        }
    }
}
