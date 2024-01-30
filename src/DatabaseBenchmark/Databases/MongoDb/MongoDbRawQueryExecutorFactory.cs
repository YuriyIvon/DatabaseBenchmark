using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.MongoDb.Interfaces;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Model;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DatabaseBenchmark.Databases.MongoDb
{
    public class MongoDbRawQueryExecutorFactory : QueryExecutorFactoryBase
    {
        public MongoDbRawQueryExecutorFactory(
            IDatabase database,
            RawQuery query,
            IExecutionEnvironment environment,
            IOptionsProvider optionsProvider)
        {
            Container.RegisterInstance<IDatabase>(database);
            Container.RegisterInstance<RawQuery>(query);
            Container.RegisterInstance<IExecutionEnvironment>(environment);
            Container.RegisterInstance<IOptionsProvider>(optionsProvider);
            Container.RegisterSingleton<IColumnPropertiesProvider, RawQueryColumnPropertiesProvider>();
            Container.RegisterSingleton<IGeneratorFactory, DummyGeneratorFactory>();
            Container.RegisterSingleton<IRandomPrimitives, RandomPrimitives>();
            Container.RegisterSingleton<ICache, MemoryCache>();
            Container.RegisterDecorator<IDistinctValuesProvider, CachedDistinctValuesProvider>(Lifestyle);

            Container.Register<IMongoDatabase>(() =>
            {
                var client = new MongoClient(database.ConnectionString);
                var databaseName = MongoUrl.Create(database.ConnectionString).DatabaseName;
                return client.GetDatabase(databaseName);
            },
                Lifestyle);
            Container.Register<IMongoCollection<BsonDocument>>(() =>
                Container.GetInstance<IMongoDatabase>().GetCollection<BsonDocument>(query.TableName),
                Lifestyle);
            Container.Register<IDistinctValuesProvider, MongoDbDistinctValuesProvider>(Lifestyle);
            Container.Register<IRandomValueProvider, RandomValueProvider>(Lifestyle);
            Container.Register<IMongoDbQueryBuilder, MongoDbRawQueryBuilder>(Lifestyle);
            Container.Register<IQueryExecutor, MongoDbQueryExecutor>(Lifestyle);
        }
    }
}
