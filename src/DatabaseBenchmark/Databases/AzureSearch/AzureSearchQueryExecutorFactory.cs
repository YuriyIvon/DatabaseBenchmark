using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.AzureSearch.Interfaces;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Model;
using Azure.Search.Documents;

namespace DatabaseBenchmark.Databases.AzureSearch
{
    public class AzureSearchQueryExecutorFactory : QueryExecutorFactoryBase
    {
        public AzureSearchQueryExecutorFactory(
            IDatabase database,
            Func<SearchClient> createClient,
            Table table,
            Query query,
            IExecutionEnvironment environment)
        {
            Container.RegisterInstance<IDatabase>(database);
            Container.RegisterInstance<Table>(table);
            Container.RegisterInstance<Query>(query);
            Container.RegisterInstance<IExecutionEnvironment>(environment);
            Container.RegisterSingleton<IGeneratorFactory, DummyGeneratorFactory>();
            Container.RegisterSingleton<IRandomPrimitives, RandomPrimitives>();
            Container.RegisterSingleton<ICache, MemoryCache>();
            Container.RegisterDecorator<IDistinctValuesProvider, CachedDistinctValuesProvider>(Lifestyle);

            Container.Register<SearchClient>(createClient, Lifestyle);
            Container.Register<IDistinctValuesProvider, AzureSearchDistinctValuesProvider>(Lifestyle);
            Container.Register<IRandomValueProvider, RandomValueProvider>(Lifestyle);
            Container.Register<IAzureSearchQueryBuilder, AzureSearchQueryBuilder>(Lifestyle);
            Container.Register<IQueryExecutor, AzureSearchQueryExecutor>(Lifestyle);
        }
    }
}