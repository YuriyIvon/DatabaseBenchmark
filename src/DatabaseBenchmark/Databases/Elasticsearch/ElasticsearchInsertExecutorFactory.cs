using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Elasticsearch.Interfaces;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using Nest;
using SimpleInjector;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public class ElasticsearchInsertExecutorFactory : QueryExecutorFactoryBase
    {
        public ElasticsearchInsertExecutorFactory(
            Func<ElasticClient> createClient,
            Table table,
            IDataSource source)
        {
            Container.RegisterInstance<Table>(table);
            Container.RegisterInstance<IDataSource>(source);

            Container.Register<IElasticClient>(createClient, Lifestyle);
            Container.Register<IElasticsearchInsertBuilder, ElasticsearchInsertBuilder>(Lifestyle);
            Container.Register<IQueryExecutor, ElasticsearchInsertExecutor>(Lifestyle);
        }
    }
}
