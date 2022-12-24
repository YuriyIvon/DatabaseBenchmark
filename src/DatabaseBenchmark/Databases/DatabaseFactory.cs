using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.ClickHouse;
using DatabaseBenchmark.Databases.CosmosDb;
using DatabaseBenchmark.Databases.Elasticsearch;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Databases.MonetDb;
using DatabaseBenchmark.Databases.MongoDb;
using DatabaseBenchmark.Databases.MySql;
using DatabaseBenchmark.Databases.Oracle;
using DatabaseBenchmark.Databases.PostgreSql;
using DatabaseBenchmark.Databases.Snowflake;
using DatabaseBenchmark.Databases.SqlServer;

namespace DatabaseBenchmark.Databases
{
    public class DatabaseFactory : IDatabaseFactory, IAllowedValuesProvider
    {
        private readonly Dictionary<string, Func<string, IDatabase>> _factories;

        public IEnumerable<string> Options => _factories.Keys;

        public DatabaseFactory(IExecutionEnvironment environment, IOptionsProvider optionsProvider)
        {
            _factories = new()
            {
                ["SqlServer"] = (connectionString) => new SqlServerDatabase(connectionString, environment),
                ["Postgres"] = (connectionString) => new PostgreSqlDatabase(connectionString, environment),
                ["PostgresJsonb"] = (connectionString) => new PostgreSqlJsonbDatabase(connectionString, environment, optionsProvider),
                ["Elasticsearch"] = (connectionString) => new ElasticsearchDatabase(connectionString, environment),
                ["MySql"] = (connectionString) => new MySqlDatabase(connectionString, environment, optionsProvider),
                ["MonetDb"] = (connectionString) => new MonetDbDatabase(connectionString, environment),
                ["MongoDb"] = (connectionString) => new MongoDbDatabase(connectionString, environment, optionsProvider),
                ["CosmosDb"] = (connectionString) => new CosmosDbDatabase(connectionString, environment, optionsProvider),
                ["ClickHouse"] = (connectionString) => new ClickHouseDatabase(connectionString, environment, optionsProvider),
                ["Oracle"] = (connectionString) => new OracleDatabase(connectionString, environment),
                ["Snowflake"] = (connectionString) => new SnowflakeDatabase(connectionString, environment)
            };
        }

        public IDatabase Create(string type, string connectionString)
        {
            if (_factories.TryGetValue(type, out var factory))
            {
                return factory(connectionString);
            }

            throw new InputArgumentException($"Unknown database type \"{type}\"");
        }
    }
}
