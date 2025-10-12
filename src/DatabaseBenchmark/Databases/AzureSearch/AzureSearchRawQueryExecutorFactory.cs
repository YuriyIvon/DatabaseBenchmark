using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.AzureSearch.Interfaces;
using DatabaseBenchmark.Generators.Interfaces;
using Azure.Search.Documents;
using RawQuery = DatabaseBenchmark.Model.RawQuery;

namespace DatabaseBenchmark.Databases.AzureSearch
{
    public class AzureSearchRawQueryExecutorFactory : QueryExecutorFactoryBase
    {
        public AzureSearchRawQueryExecutorFactory(
            IDatabase database,
            Func<SearchClient> createClient,
            RawQuery query,
            IExecutionEnvironment environment)
        {
            Container.RegisterInstance<IDatabase>(database);
            Container.RegisterInstance<RawQuery>(query);
            Container.RegisterInstance<IExecutionEnvironment>(environment);
            Container.RegisterSingleton<IGeneratorFactory, DummyGeneratorFactory>();
            Container.RegisterSingleton<IRandomPrimitives, RandomPrimitives>();
            Container.RegisterSingleton<ICache, MemoryCache>();
            Container.RegisterDecorator<IDistinctValuesProvider, CachedDistinctValuesProvider>(Lifestyle);

            Container.Register<SearchClient>(createClient, Lifestyle);
            Container.Register<IDistinctValuesProvider, AzureSearchDistinctValuesProvider>(Lifestyle);
            Container.Register<IRandomValueProvider, RandomValueProvider>(Lifestyle);
            Container.Register<IAzureSearchQueryBuilder, AzureSearchRawQueryBuilder>(Lifestyle);
            Container.Register<IQueryExecutor, AzureSearchQueryExecutor>(Lifestyle);
        }
    }
}