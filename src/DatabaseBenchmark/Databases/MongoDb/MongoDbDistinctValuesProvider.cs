using DatabaseBenchmark.Core.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DatabaseBenchmark.Databases.MongoDb
{
    public class MongoDbDistinctValuesProvider : IDistinctValuesProvider
    {
        private readonly IMongoDatabase _database;
        private readonly IExecutionEnvironment _environment;

        public MongoDbDistinctValuesProvider(
            IMongoDatabase database,
            IExecutionEnvironment environment)
        {
            _database = database;
            _environment = environment;
        }

        public object[] GetDistinctValues(string tableName, string columnName)
        {
            var collection = _database.GetCollection<BsonDocument>(tableName);
            FieldDefinition<BsonDocument, object> field = columnName;

            if (_environment.TraceQueries)
            {
                _environment.WriteLine($"Reading distinct values for column {columnName} of collection {tableName}");
            }

            var result = collection.Distinct(field, new BsonDocument());
            return result.ToList().ToArray();
        }
    }
}
