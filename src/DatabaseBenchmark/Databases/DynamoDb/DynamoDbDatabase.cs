using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.DynamoDb.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.DynamoDb
{
    public class DynamoDbDatabase : IDatabase
    {
        private const int DefaultImportBatchSize = 25;

        private const string AccessKeyIdConnectionStringProperty = "AccessKeyId";
        private const string SecretAccessKeyConnectionStringProperty = "SecretAccessKey";
        private const string RegionEndpointConnectionStringProperty = "RegionEndpoint";

        private readonly IExecutionEnvironment _environment;

        public string ConnectionString { get; }

        public DynamoDbDatabase(string connectionString, IExecutionEnvironment environment)
        {
            ConnectionString = connectionString;
            _environment = environment;
        }

        public void CreateTable(Table table, bool dropExisting)
        {
            if (table.Columns.Any(c => c.DatabaseGenerated))
            {
                _environment.WriteLine("WARNING: DynamoDB doesn't support database-generated columns");
            }

            var client = CreateClient();

            if (dropExisting)
            {
                DropTable(client, table.Name);
            }

            var tableBuilder = new DynamoDbTableBuilder();
            var createTableRequest = tableBuilder.Build(table);
            var createResponse = client.CreateTableAsync(createTableRequest).Result;

            var status = createResponse.TableDescription.TableStatus;
            while (status != TableStatus.ACTIVE)
            {
                _environment.WriteLine("Waiting until the table is active");
                Thread.Sleep(1000);

                var describeResponse = client.DescribeTableAsync(createTableRequest.TableName).Result;
                status = describeResponse.Table.TableStatus;
            }
        }
        public IDataImporter CreateDataImporter(Table table, IDataSource source, int batchSize) =>
            new DataImporterBuilder(table, source, batchSize, DefaultImportBatchSize)
                .TransactionProvider<NoTransactionProvider>()
                .InsertBuilder<IDynamoDbInsertBuilder, DynamoDbInsertBuilder>()
                .InsertExecutor<DynamoDbInsertExecutor>()
                .DataMetricsProvider<DynamoDbDataMetricsProvider>()
                .ProgressReporter<ImportProgressReporter>()
                .Environment(_environment)
                .Customize((container, lifestyle) =>
                {
                    container.Register<IDynamoDbMetricsReporter>(() => (IDynamoDbMetricsReporter)container.GetInstance<IDataMetricsProvider>());
                    container.Register<AmazonDynamoDBClient>(CreateClient, lifestyle);
                })
                .Build();

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query) =>
            new DynamoDbQueryExecutorFactory(this, CreateClient, table, query, _environment);

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            new DynamoDbRawQueryExecutorFactory(this, CreateClient, query, _environment);

        public IQueryExecutorFactory CreateInsertExecutorFactory(Table table, IDataSource dataSource, int batchSize) =>
        new DynamoDbInsertExecutorFactory(CreateClient, table, dataSource, batchSize, _environment);

        public void ExecuteScript(string script) => throw new InputArgumentException("Custom scripts are not supported for DynamoDB");

        private AmazonDynamoDBClient CreateClient()
        {
            var connectionParameters = ConnectionStringParser.Parse(
                ConnectionString,
                AccessKeyIdConnectionStringProperty,
                SecretAccessKeyConnectionStringProperty,
                RegionEndpointConnectionStringProperty);

            var clientConfig = new AmazonDynamoDBConfig
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(connectionParameters[RegionEndpointConnectionStringProperty])
            };

            return new AmazonDynamoDBClient(
                connectionParameters[AccessKeyIdConnectionStringProperty],
                connectionParameters[SecretAccessKeyConnectionStringProperty],
                clientConfig);
        }

        private static void DropTable(AmazonDynamoDBClient client, string tableName)
        {
            bool isTableDeleted = false;

            try
            {
                var deleteResponse = client.DeleteTableAsync(
                    new DeleteTableRequest { TableName = tableName }).GetAwaiter().GetResult();
            }
            catch (ResourceNotFoundException)
            {
                isTableDeleted = true;
            }

            while (!isTableDeleted)
            {
                try
                {
                    var response = client.DescribeTableAsync(tableName).GetAwaiter().GetResult();
                    Thread.Sleep(1000);
                }
                catch (ResourceNotFoundException)
                {
                    isTableDeleted = true;
                }
            }
        }
    }
}
