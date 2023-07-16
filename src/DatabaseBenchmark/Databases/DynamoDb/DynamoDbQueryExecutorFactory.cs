using Amazon.DynamoDBv2;
using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;
using SimpleInjector;

namespace DatabaseBenchmark.Databases.DynamoDb
{
    public class DynamoDbQueryExecutorFactory : QueryExecutorFactoryBase
    {
        public DynamoDbQueryExecutorFactory(
           Func<AmazonDynamoDBClient> createClient,
           Table table,
           Query query,
           IExecutionEnvironment environment)
        {
            Container.RegisterInstance<Table>(table);
            Container.RegisterInstance<Query>(query);
            Container.RegisterInstance<IExecutionEnvironment>(environment);
            Container.RegisterSingleton<IColumnPropertiesProvider, TableColumnPropertiesProvider>();
            Container.RegisterSingleton<IRandomGenerator, RandomGenerator>();
            Container.RegisterSingleton<ICache, MemoryCache>();
            Container.RegisterDecorator<IDistinctValuesProvider, CachedDistinctValuesProvider>(Lifestyle);

            Container.Register<AmazonDynamoDBClient>(createClient, Lifestyle);
            Container.Register<IDistinctValuesProvider, DynamoDbDistinctValuesProvider>(Lifestyle);
            Container.Register<IRandomValueProvider, RandomValueProvider>(Lifestyle);
            Container.Register<ISqlParametersBuilder>(() => new SqlParametersBuilder('?', true), Lifestyle);
            Container.Register<ISqlQueryBuilder, DynamoDbQueryBuilder>(Lifestyle);
            Container.Register<IQueryExecutor, DynamoDbQueryExecutor>(Lifestyle);
        }
    }
}
