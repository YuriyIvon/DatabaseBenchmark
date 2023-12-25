using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.MongoDb.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DatabaseBenchmark.Databases.MongoDb
{
    public class MongoDbInsertExecutorFactory : QueryExecutorFactoryBase
    {
        public MongoDbInsertExecutorFactory(
            IDatabase database,
            Table table,
            IDataSource source,
            int batchSize,
            IExecutionEnvironment environment,
            IOptionsProvider optionsProvider)
        {
            Container.RegisterInstance<Table>(table);
            Container.RegisterInstance<IDataSource>(source);
            Container.RegisterInstance<IExecutionEnvironment>(environment);
            Container.RegisterInstance<IOptionsProvider>(optionsProvider);

            var insertBuilderOptions = new InsertBuilderOptions { BatchSize = batchSize };
            Container.RegisterInstance<InsertBuilderOptions>(insertBuilderOptions);

            Container.RegisterSingleton<IDataSourceReader, DataSourceReader>();

            Container.Register<IMongoDatabase>(() =>
                {
                    var client = new MongoClient(database.ConnectionString);
                    var databaseName = MongoUrl.Create(database.ConnectionString).DatabaseName;
                    return client.GetDatabase(databaseName);
                },
                Lifestyle);
            Container.Register<IMongoCollection<BsonDocument>>(() =>
                Container.GetInstance<IMongoDatabase>().GetCollection<BsonDocument>(table.Name),
                Lifestyle);
            Container.Register<IMongoDbInsertBuilder, MongoDbInsertBuilder>(Lifestyle);
            Container.Register<IQueryExecutor, MongoDbInsertExecutor>(Lifestyle);
        }
    }
}
