using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Elasticsearch.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Model;
using Nest;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public class ElasticsearchQueryExecutorFactory : QueryExecutorFactoryBase
    {
        public ElasticsearchQueryExecutorFactory(
            Func<ElasticClient> createClient,
            Table table,
            Query query)
        {
            Container.RegisterInstance<Table>(table);
            Container.RegisterInstance<Query>(query);
            Container.RegisterSingleton<IColumnPropertiesProvider, TableColumnPropertiesProvider>();
            Container.RegisterSingleton<IRandomGenerator, RandomGenerator>();
            Container.RegisterSingleton<ICache, MemoryCache>();
            Container.RegisterDecorator<IDistinctValuesProvider, CachedDistinctValuesProvider>(Lifestyle);

            Container.Register<ElasticClient>(createClient, Lifestyle);
            Container.Register<IDistinctValuesProvider, ElasticsearchDistinctValuesProvider>(Lifestyle);
            Container.Register<IRandomValueProvider, RandomValueProvider>(Lifestyle);
            Container.Register<IElasticsearchQueryBuilder, ElasticsearchQueryBuilder>(Lifestyle);
            Container.Register<IQueryExecutor, ElasticsearchQueryExecutor>(Lifestyle);
        }
    }
}
