using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.MongoDb.Interfaces;
using DatabaseBenchmark.Model;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DatabaseBenchmark.Databases.MongoDb
{
    public class MongoDbRawQueryExecutorFactory : QueryExecutorFactoryBase
    {
        public MongoDbRawQueryExecutorFactory(
            string connectionString,
            RawQuery query,
            IExecutionEnvironment environment,
            IOptionsProvider optionsProvider)
        {
            Container.RegisterInstance<RawQuery>(query);
            Container.RegisterInstance<IExecutionEnvironment>(environment);
            Container.RegisterInstance<IOptionsProvider>(optionsProvider);
            Container.RegisterSingleton<IColumnPropertiesProvider, RawQueryColumnPropertiesProvider>();
            Container.RegisterSingleton<IRandomGenerator, RandomGenerator>();
            Container.RegisterSingleton<ICache, MemoryCache>();
            Container.RegisterDecorator<IDistinctValuesProvider, CachedDistinctValuesProvider>(Lifestyle);

            Container.Register<IMongoDatabase>(() =>
            {
                var client = new MongoClient(connectionString);
                var databaseName = MongoUrl.Create(connectionString).DatabaseName;
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
