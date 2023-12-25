using Amazon.DynamoDBv2;
using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Generators;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Model;
using SimpleInjector;

namespace DatabaseBenchmark.Databases.DynamoDb
{
    public class DynamoDbRawQueryExecutorFactory : QueryExecutorFactoryBase
    {
        public DynamoDbRawQueryExecutorFactory(
            IDatabase database,
            Func<AmazonDynamoDBClient> createClient,
            RawQuery query,
            IExecutionEnvironment environment)
        {
            Container.RegisterInstance<IDatabase>(database);
            Container.RegisterInstance<RawQuery>(query);
            Container.RegisterInstance<IExecutionEnvironment>(environment);
            Container.RegisterSingleton<IColumnPropertiesProvider, RawQueryColumnPropertiesProvider>();
            Container.RegisterSingleton<IGeneratorFactory, GeneratorFactory>();
            Container.RegisterSingleton<IRandomPrimitives, RandomPrimitives>();
            Container.RegisterSingleton<ICache, MemoryCache>();
            Container.RegisterDecorator<IDistinctValuesProvider, CachedDistinctValuesProvider>(Lifestyle);

            Container.Register<AmazonDynamoDBClient>(createClient, Lifestyle);
            Container.Register<IDistinctValuesProvider, DynamoDbDistinctValuesProvider>(Lifestyle);
            Container.Register<IRandomValueProvider, RandomValueProvider>(Lifestyle);
            Container.Register<ISqlParametersBuilder>(() => new SqlParametersBuilder('?', true), Lifestyle);
            Container.Register<ISqlQueryBuilder, SqlRawQueryBuilder>(Lifestyle);
            Container.Register<IQueryExecutor, DynamoDbQueryExecutor>(Lifestyle);
        }
    }
}
