using DatabaseBenchmark.Commands;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Interfaces;
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
            string connectionString,
            Table table,
            IDataSource source,
            IOptionsProvider optionsProvider)
        {
            Container.RegisterInstance<Table>(table);
            Container.RegisterInstance<IDataSource>(source);
            Container.RegisterInstance<IOptionsProvider>(optionsProvider);

            Container.RegisterSingleton<IDataSourceReader, DataSourceReader>();

            Container.Register<IMongoDatabase>(() =>
                {
                    var client = new MongoClient(connectionString);
                    var databaseName = MongoUrl.Create(connectionString).DatabaseName;
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
