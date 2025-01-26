using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Elasticsearch.Interfaces;
using DatabaseBenchmark.Generators.Interfaces;
using Elasticsearch.Net;
using Nest;
using RawQuery = DatabaseBenchmark.Model.RawQuery;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public class ElasticsearchRawQueryExecutorFactory : QueryExecutorFactoryBase
    {
        public ElasticsearchRawQueryExecutorFactory(
            IDatabase database,
            Func<ElasticClient> createClient,
            RawQuery query)
        {
            Container.RegisterInstance<IDatabase>(database);
            Container.RegisterInstance<RawQuery>(query);
            Container.RegisterSingleton<IGeneratorFactory, DummyGeneratorFactory>();
            Container.RegisterSingleton<IRandomPrimitives, RandomPrimitives>();
            Container.RegisterSingleton<ICache, MemoryCache>();
            Container.RegisterDecorator<IDistinctValuesProvider, CachedDistinctValuesProvider>(Lifestyle);

            Container.Register<ElasticClient>(createClient, Lifestyle);
            //TODO: Find a better way to instantiate the default serializer
            Container.Register<IElasticsearchSerializer>(() => Container.GetInstance<ElasticClient>().RequestResponseSerializer, Lifestyle);
            Container.Register<IDistinctValuesProvider, ElasticsearchDistinctValuesProvider>(Lifestyle);
            Container.Register<IRandomValueProvider, RandomValueProvider>(Lifestyle);
            Container.Register<IElasticsearchQueryBuilder, ElasticsearchRawQueryBuilder>(Lifestyle);
            Container.Register<IQueryExecutor, ElasticsearchQueryExecutor>(Lifestyle);
        }
    }
}
