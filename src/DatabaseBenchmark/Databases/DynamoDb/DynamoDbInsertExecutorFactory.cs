using Amazon.DynamoDBv2;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.DynamoDb.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using SimpleInjector;

namespace DatabaseBenchmark.Databases.DynamoDb
{
    public class DynamoDbInsertExecutorFactory : QueryExecutorFactoryBase
    {
        public DynamoDbInsertExecutorFactory(
            Func<AmazonDynamoDBClient> createClient,
            Table table,
            IDataSource source,
            int batchSize,
            IExecutionEnvironment environment)
        {
            Container.RegisterInstance<Table>(table);
            Container.RegisterInstance<IDataSource>(source);
            Container.RegisterInstance<IExecutionEnvironment>(environment);

            var insertBuilderOptions = new InsertBuilderOptions { BatchSize = batchSize };
            Container.RegisterInstance<InsertBuilderOptions>(insertBuilderOptions);

            Container.RegisterSingleton<IDataSourceReader, DataSourceReader>();

            Container.Register<AmazonDynamoDBClient>(createClient, Lifestyle);
            Container.Register<IDynamoDbInsertBuilder, DynamoDbInsertBuilder>(Lifestyle);
            Container.Register<IQueryExecutor, DynamoDbInsertExecutor>(Lifestyle);
        }
    }
}
