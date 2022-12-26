using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;
using Microsoft.Azure.Cosmos;

namespace DatabaseBenchmark.Databases.CosmosDb
{
    public class CosmosDbRawQueryExecutorFactory : QueryExecutorFactoryBase
    {
        public CosmosDbRawQueryExecutorFactory( 
            string connectionString,
            string databaseName,
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

            Container.Register<CosmosClient>(() => new CosmosClient(connectionString), Lifestyle);
            Container.Register<Database>(() => Container.GetInstance<CosmosClient>().GetDatabase(databaseName), Lifestyle);
            Container.Register<Container>(() => Container.GetInstance<Database>().GetContainer(query.TableName), Lifestyle);
            Container.Register<IDistinctValuesProvider, CosmosDbDistinctValuesProvider>(Lifestyle);
            Container.Register<IRandomValueProvider, RandomValueProvider>(Lifestyle);
            Container.Register<ISqlParametersBuilder, SqlParametersBuilder>(Lifestyle);
            Container.Register<ISqlQueryBuilder, SqlRawQueryBuilder>(Lifestyle);
            Container.Register<IQueryExecutor, CosmosDbQueryExecutor>(Lifestyle);
        }
    }
}
