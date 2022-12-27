using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.CosmosDb.Interfaces;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using Microsoft.Azure.Cosmos;

namespace DatabaseBenchmark.Databases.CosmosDb
{
    public class CosmosDbInsertExecutorFactory : QueryExecutorFactoryBase
    {
        public CosmosDbInsertExecutorFactory(
            string connectionString,
            string databaseName,
            Table table,
            IDataSource source)
        {
            Container.RegisterInstance<Table>(table);
            Container.RegisterInstance<IDataSource>(source);

            Container.RegisterSingleton<IDataSourceReader, DataSourceReader>();

            Container.Register<CosmosClient>(() => new CosmosClient(connectionString), Lifestyle);
            Container.Register<Database>(() => Container.GetInstance<CosmosClient>().GetDatabase(databaseName), Lifestyle);
            Container.Register<Container>(() => Container.GetInstance<Database>().GetContainer(table.Name), Lifestyle);
            Container.Register<ICosmosDbInsertBuilder, CosmosDbInsertBuilder>(Lifestyle);
            Container.Register<IQueryExecutor, CosmosDbInsertExecutor>(Lifestyle);
        }
    }
}
