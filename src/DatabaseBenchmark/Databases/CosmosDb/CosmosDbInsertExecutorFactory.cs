using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.CosmosDb.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using Microsoft.Azure.Cosmos;

namespace DatabaseBenchmark.Databases.CosmosDb
{
    public class CosmosDbInsertExecutorFactory : QueryExecutorFactoryBase
    {
        public CosmosDbInsertExecutorFactory(
            IDatabase database,
            string databaseName,
            Table table,
            IDataSource source,
            int batchSize,
            IExecutionEnvironment environment)
        {
            Container.RegisterInstance<Table>(table);
            Container.RegisterInstance<IDataSource>(source);
            Container.RegisterInstance<IExecutionEnvironment>(environment);

            var insertBuilderOptions = new InsertBuilderOptions { BatchSize = batchSize };
            Container.RegisterInstance<InsertBuilderOptions>(insertBuilderOptions);

            Container.RegisterSingleton<IDataSourceReader, DataSourceReader>();

            Container.Register<CosmosClient>(() => new CosmosClient(database.ConnectionString), Lifestyle);
            Container.Register<Database>(() => Container.GetInstance<CosmosClient>().GetDatabase(databaseName), Lifestyle);
            Container.Register<Container>(() => Container.GetInstance<Database>().GetContainer(table.Name), Lifestyle);
            Container.Register<ICosmosDbInsertBuilder, CosmosDbInsertBuilder>(Lifestyle);
            Container.Register<IQueryExecutor, CosmosDbInsertExecutor>(Lifestyle);
        }
    }
}
