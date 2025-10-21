using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using System.Text;

namespace DatabaseBenchmark.Databases.DynamoDb
{
    public class DynamoDbQueryExecutor : IQueryExecutor
    {
        private readonly AmazonDynamoDBClient _client;
        private readonly ISqlParametersBuilder _parametersBuilder;
        private readonly ISqlQueryBuilder _queryBuilder;
        private readonly IExecutionEnvironment _environment;

        public DynamoDbQueryExecutor(
            AmazonDynamoDBClient client,
            ISqlQueryBuilder queryBuilder,
            ISqlParametersBuilder parametersBuilder,
            IExecutionEnvironment environment)
        {
            _client = client;
            _queryBuilder = queryBuilder;
            _parametersBuilder = parametersBuilder;
            _environment = environment;
        }

        public IPreparedQuery Prepare()
        {
            var query = _queryBuilder.Build();

            _environment.TraceCommand(query, _parametersBuilder.Parameters);

            var request = new ExecuteStatementRequest
            {
                Statement = query,
                ReturnConsumedCapacity = ReturnConsumedCapacity.TOTAL,
                Parameters = _parametersBuilder.Parameters.Any()
                    ? _parametersBuilder.Parameters
                        .Select(p => DynamoDbAttributeValueUtils.ToAttributeValue(p.Type, p.Array, p.Value))
                        .ToList()
                    : null
            };

            return new DynamoDbPreparedQuery(_client, request);
        }

        public IPreparedQuery Prepare(ITransaction transaction) => Prepare();

        public void Dispose() => _client?.Dispose();
    }
}
