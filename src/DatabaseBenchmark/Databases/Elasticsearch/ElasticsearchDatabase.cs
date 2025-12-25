using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Elasticsearch.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using Nest;
using System.Text;
using RawQuery = DatabaseBenchmark.Model.RawQuery;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public class ElasticsearchDatabase : IDatabase
    {
        private const int DefaultImportBatchSize = 500;

        private readonly IExecutionEnvironment _environment;

        public string ConnectionString { get; }

        public ElasticsearchDatabase(string connectionString, IExecutionEnvironment environment)
        {
            ConnectionString = connectionString;
            _environment = environment;
        }

        public void CreateTable(Table table, bool dropExisting)
        {
            table = NormalizeNames(table);

            var client = CreateClient();

            if (dropExisting)
            {
                var exists = client.Indices.Exists(table.Name);
                if (exists.Exists)
                {
                    client.Indices.Delete(table.Name);
                }
            }

            client.Indices.CreateAsync(table.Name, ci => ci
                .Map(md => md
                    .Properties(pd => BuildProperties(table, pd)))).Wait();
        }

        public IDataImporter CreateDataImporter(Table table, IDataSource source, int batchSize)
        {
            table = NormalizeNames(table);

            return new DataImporterBuilder(table, source, batchSize, DefaultImportBatchSize)
                .TransactionProvider<NoTransactionProvider>()
                .InsertBuilder<IElasticsearchInsertBuilder, ElasticsearchInsertBuilder>()
                .InsertExecutor<ElasticsearchInsertExecutor>()
                .DataMetricsProvider<ElasticsearchDataMetricsProvider>()
                .ProgressReporter<ImportProgressReporter>()
                .Environment(_environment)
                .Customize((container, lifestyle) =>
                {
                    container.Register<IElasticClient>(CreateClient, lifestyle);
                })
                .Build();
        }

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query) =>
            new ElasticsearchQueryExecutorFactory(this, CreateClient, NormalizeNames(table), query);

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            new ElasticsearchRawQueryExecutorFactory(this, CreateClient, query);

        public IQueryExecutorFactory CreateInsertExecutorFactory(Table table, IDataSource source, int batchSize) =>
            new ElasticsearchInsertExecutorFactory(CreateClient, table, source, batchSize);

        public void ExecuteScript(string script) => throw new InputArgumentException("Custom scripts are not supported for Elasticsearch");

        private ElasticClient CreateClient()
        {
            var connectionSettings = new ConnectionSettings(new Uri(ConnectionString))
                .ThrowExceptions();

            if (_environment.TraceQueries || _environment.TraceResults)
            {
                connectionSettings.EnableDebugMode(response =>
                {
                    if (_environment.TraceQueries && response.RequestBodyInBytes != null)
                    {
                        _environment.WriteLine(Encoding.UTF8.GetString(response.RequestBodyInBytes));
                    }

                    if (_environment.TraceResults && response.ResponseBodyInBytes != null)
                    {
                        _environment.WriteLine(Encoding.UTF8.GetString(response.ResponseBodyInBytes));
                    }
                });
            }

            return new ElasticClient(connectionSettings);
        }

        private PropertiesDescriptor<object> BuildProperties(Table table, PropertiesDescriptor<object> propertiesDescriptor)
        {
            foreach (var column in table.Columns)
            {
                if (column.DatabaseGenerated)
                {
                    _environment.WriteLine("WARNING: Elasticsearch doesn't support database-generated columns");
                    continue;
                }

                switch (column.Type)
                {
                    case ColumnType.Boolean:
                        propertiesDescriptor.Boolean(bpd => bpd
                            .Name(column.Name)
                            .Index(column.Queryable));
                        break;
                    case ColumnType.Guid:
                    case ColumnType.String:
                        propertiesDescriptor.Keyword(kpd => kpd
                            .Name(column.Name)
                            .Index(column.Queryable));
                        break;
                    case ColumnType.Double:
                        propertiesDescriptor.Number(npd => npd
                            .Name(column.Name)
                            .Type(NumberType.Double)
                            .Index(column.Queryable));
                        break;
                    case ColumnType.Integer:
                        propertiesDescriptor.Number(npd => npd
                            .Name(column.Name)
                            .Type(NumberType.Integer)
                            .Index(column.Queryable));
                        break;
                    case ColumnType.Text:
                        propertiesDescriptor.Text(tpd => tpd
                            .Name(column.Name)
                            .Index(column.Queryable));
                        break;
                    case ColumnType.DateTime:
                        propertiesDescriptor.Date(dpd => dpd
                            .Name(column.Name)
                            .Index(column.Queryable));
                        break;
                    case ColumnType.Vector:
                        propertiesDescriptor.DenseVector(vpd => vpd
                            .Name(column.Name)
                            .Dimensions(column.Size));
                        break;
                    default:
                        throw new InputArgumentException($"Unknown column type \"{column.Type}\"");
                }
            }

            return propertiesDescriptor;
        }

        private static Table NormalizeNames(Table table)
        {
            table.Name = table.Name.ToLower();
            return table;
        }
    }
}
