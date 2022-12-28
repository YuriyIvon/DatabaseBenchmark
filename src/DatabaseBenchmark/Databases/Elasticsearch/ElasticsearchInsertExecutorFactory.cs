using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Elasticsearch.Interfaces;
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
            IDataSource source,
            int batchSize)
        {
            Container.RegisterInstance<Table>(table);
            Container.RegisterInstance<IDataSource>(source);

            var insertBuilderOptions = new InsertBuilderOptions { BatchSize = batchSize };
            Container.RegisterInstance<InsertBuilderOptions>(insertBuilderOptions);

            Container.RegisterSingleton<IDataSourceReader, DataSourceReader>();

            Container.Register<IElasticClient>(createClient, Lifestyle);
            Container.Register<IElasticsearchInsertBuilder, ElasticsearchInsertBuilder>(Lifestyle);
            Container.Register<IQueryExecutor, ElasticsearchInsertExecutor>(Lifestyle);
        }
    }
}
