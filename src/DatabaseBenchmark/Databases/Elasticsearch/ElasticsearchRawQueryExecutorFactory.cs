using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Elasticsearch.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using Nest;
using RawQuery = DatabaseBenchmark.Model.RawQuery;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public class ElasticsearchRawQueryExecutorFactory : QueryExecutorFactoryBase
    {
        public ElasticsearchRawQueryExecutorFactory(
            Func<ElasticClient> createClient,
            RawQuery query)
        {
            Container.RegisterInstance<RawQuery>(query);
            Container.RegisterSingleton<IColumnPropertiesProvider, RawQueryColumnPropertiesProvider>();
            Container.RegisterSingleton<IRandomGenerator, RandomGenerator>();
            Container.RegisterSingleton<ICache, MemoryCache>();
            Container.RegisterDecorator<IDistinctValuesProvider, CachedDistinctValuesProvider>(Lifestyle);

            Container.Register<ElasticClient>(createClient, Lifestyle);
            Container.Register<IDistinctValuesProvider, ElasticsearchDistinctValuesProvider>(Lifestyle);
            Container.Register<IRandomValueProvider, RandomValueProvider>(Lifestyle);
            Container.Register<IElasticsearchQueryBuilder, ElasticsearchRawQueryBuilder>(Lifestyle);
            Container.Register<IQueryExecutor, ElasticsearchQueryExecutor>(Lifestyle);
        }
    }
}
