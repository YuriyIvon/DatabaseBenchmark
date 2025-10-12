using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.AzureSearch.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using Azure.Search.Documents;

namespace DatabaseBenchmark.Databases.AzureSearch
{
    public class AzureSearchInsertExecutorFactory : QueryExecutorFactoryBase
    {
        public AzureSearchInsertExecutorFactory(
            Func<SearchClient> createClient,
            Table table,
            IDataSource source,
            int batchSize)
        {
            Container.RegisterInstance<Table>(table);
            Container.RegisterInstance<IDataSource>(source);

            var insertBuilderOptions = new InsertBuilderOptions { BatchSize = batchSize };
            Container.RegisterInstance<InsertBuilderOptions>(insertBuilderOptions);

            Container.RegisterSingleton<IDataSourceReader, DataSourceReader>();

            Container.Register<SearchClient>(createClient, Lifestyle);
            Container.Register<IAzureSearchInsertBuilder, AzureSearchInsertBuilder>(Lifestyle);
            Container.Register<IQueryExecutor, AzureSearchInsertExecutor>(Lifestyle);
        }
    }
}