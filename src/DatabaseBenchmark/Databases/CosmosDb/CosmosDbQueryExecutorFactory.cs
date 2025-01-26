using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Model;
using Microsoft.Azure.Cosmos;

namespace DatabaseBenchmark.Databases.CosmosDb
{
    public class CosmosDbQueryExecutorFactory : QueryExecutorFactoryBase
    {
        public CosmosDbQueryExecutorFactory(
            IDatabase database,
            string databaseName,
            Table table,
            Query query,
            IExecutionEnvironment environment,
            IOptionsProvider optionsProvider)
        {
            Container.RegisterInstance<IDatabase>(database);
            Container.RegisterInstance<Table>(table);
            Container.RegisterInstance<Query>(query);
            Container.RegisterInstance<IExecutionEnvironment>(environment);
            Container.RegisterInstance<IOptionsProvider>(optionsProvider);
            Container.RegisterSingleton<IGeneratorFactory, DummyGeneratorFactory>();
            Container.RegisterSingleton<IRandomPrimitives, RandomPrimitives>();
            Container.RegisterSingleton<ICache, MemoryCache>();
            Container.RegisterDecorator<IDistinctValuesProvider, CachedDistinctValuesProvider>(Lifestyle);

            Container.Register<CosmosClient>(() => new CosmosClient(database.ConnectionString), Lifestyle);
            Container.Register<Database>(() => Container.GetInstance<CosmosClient>().GetDatabase(databaseName), Lifestyle);
            Container.Register<Container>(() => Container.GetInstance<Database>().GetContainer(table.Name), Lifestyle);
            Container.Register<IDistinctValuesProvider, CosmosDbDistinctValuesProvider>(Lifestyle);
            Container.Register<IRandomValueProvider, RandomValueProvider>(Lifestyle);
            Container.Register<ISqlParametersBuilder>(() => new SqlParametersBuilder(), Lifestyle);
            Container.Register<ISqlQueryBuilder, CosmosDbQueryBuilder>(Lifestyle);
            Container.Register<IQueryExecutor, CosmosDbQueryExecutor>(Lifestyle);
        }
    }
}
