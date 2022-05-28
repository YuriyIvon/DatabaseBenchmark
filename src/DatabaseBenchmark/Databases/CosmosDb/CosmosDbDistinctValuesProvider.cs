using DatabaseBenchmark.Core.Interfaces;
using Microsoft.Azure.Cosmos;

namespace DatabaseBenchmark.Databases.CosmosDb
{
    public class CosmosDbDistinctValuesProvider : IDistinctValuesProvider
    {
        private readonly Database _database;
        private readonly IExecutionEnvironment _environment;

        public CosmosDbDistinctValuesProvider(
            Database database,
            IExecutionEnvironment environment)
        {
            _database = database;
            _environment = environment;
        }

        public List<object> GetDistinctValues(string tableName, string columnName)
        {
            var container = _database.GetContainer(tableName);
            var query = $"SELECT DISTINCT VALUE c.{columnName} FROM c";

            if (_environment.TraceQueries)
            {
                _environment.WriteLine(query);
            }

            return container.Query<object>(new PartitionKey(CosmosDbConstants.DummyPartitionKeyValue), query).Items;
        }
    }
}
