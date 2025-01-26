using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Model;
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

        public object[] GetDistinctValues(string tableName, IValueDefinition column, bool unfoldArray)
        {
            var container = _database.GetContainer(tableName);
            var query = unfoldArray
                ? $"SELECT DISTINCT VALUE v FROM c JOIN v IN c.{column.Name}"
                : $"SELECT DISTINCT VALUE c.{column.Name} FROM c";

            if (_environment.TraceQueries)
            {
                _environment.WriteLine(query);
            }

            return container.Query<object>(query).Items.ToArray();
        }
    }
}
