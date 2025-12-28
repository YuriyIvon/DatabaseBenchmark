using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Elasticsearch.Interfaces;
using DatabaseBenchmark.Generators.Interfaces;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using RawQuery = DatabaseBenchmark.Model.RawQuery;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public class ElasticsearchRawQueryExecutorFactory : QueryExecutorFactoryBase
    {
        public ElasticsearchRawQueryExecutorFactory(
            IDatabase database,
            Func<ElasticsearchClient> createClient,
            RawQuery query)
        {
            Container.RegisterInstance<IDatabase>(database);
            Container.RegisterInstance<RawQuery>(query);
            Container.RegisterSingleton<IGeneratorFactory, DummyGeneratorFactory>();
            Container.RegisterSingleton<IRandomPrimitives, RandomPrimitives>();
            Container.RegisterSingleton<ICache, MemoryCache>();
            Container.RegisterDecorator<IDistinctValuesProvider, CachedDistinctValuesProvider>(Lifestyle);

            Container.Register<ElasticsearchClient>(createClient, Lifestyle);
            //TODO: Find a better way to instantiate the default serializer
            Container.Register<Serializer>(() => Container.GetInstance<ElasticsearchClient>().RequestResponseSerializer, Lifestyle);
            Container.Register<IDistinctValuesProvider, ElasticsearchDistinctValuesProvider>(Lifestyle);
            Container.Register<IRandomValueProvider, RandomValueProvider>(Lifestyle);
            Container.Register<IElasticsearchQueryBuilder, ElasticsearchRawQueryBuilder>(Lifestyle);
            Container.Register<IQueryExecutor, ElasticsearchQueryExecutor>(Lifestyle);
        }
    }
}
