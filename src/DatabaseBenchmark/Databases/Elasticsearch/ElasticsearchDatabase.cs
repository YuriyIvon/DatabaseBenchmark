using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Elasticsearch.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;
using System.Text;
using RawQuery = DatabaseBenchmark.Model.RawQuery;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public class ElasticsearchDatabase : IDatabase
    {
        private const int DefaultImportBatchSize = 500;

        private readonly IExecutionEnvironment _environment;
        private readonly IOptionsProvider _optionsProvider;

        public string ConnectionString { get; }

        public ElasticsearchDatabase(
            string connectionString,
            IExecutionEnvironment environment,
            IOptionsProvider optionsProvider)
        {
            ConnectionString = connectionString;
            _environment = environment;
            _optionsProvider = optionsProvider;
        }

        public void CreateTable(Table table, bool dropExisting)
        {
            table = NormalizeNames(table);

            var client = CreateClient();

            if (dropExisting)
            {
                var existsResponse = client.Indices.ExistsAsync(table.Name).GetAwaiter().GetResult();
                if (existsResponse.Exists)
                {
                    client.Indices.DeleteAsync(table.Name).GetAwaiter().GetResult();
                }
            }

            client.Indices.CreateAsync(table.Name, c => c
                .Mappings(m => m
                    .Properties(BuildProperties(table)))).GetAwaiter().GetResult();
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
                    container.Register<ElasticsearchClient>(CreateClient, lifestyle);
                })
                .Build();
        }

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query) =>
            new ElasticsearchQueryExecutorFactory(this, CreateClient, NormalizeNames(table), query, _optionsProvider);

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            new ElasticsearchRawQueryExecutorFactory(this, CreateClient, query);

        public IQueryExecutorFactory CreateInsertExecutorFactory(Table table, IDataSource source, int batchSize) =>
            new ElasticsearchInsertExecutorFactory(CreateClient, table, source, batchSize);

        public void ExecuteScript(string script) => throw new InputArgumentException("Custom scripts are not supported for Elasticsearch");

        private ElasticsearchClient CreateClient()
        {
            var settings = new ElasticsearchClientSettings(new Uri(ConnectionString))
                .ThrowExceptions();

            if (_environment.TraceQueries || _environment.TraceResults)
            {
                settings.EnableDebugMode(details =>
                {
                    if (_environment.TraceQueries && details.RequestBodyInBytes != null)
                    {
                        _environment.WriteLine(Encoding.UTF8.GetString(details.RequestBodyInBytes));
                    }

                    if (_environment.TraceResults && details.ResponseBodyInBytes != null)
                    {
                        _environment.WriteLine(Encoding.UTF8.GetString(details.ResponseBodyInBytes));
                    }
                });
            }

            return new ElasticsearchClient(settings);
        }

        private Properties BuildProperties(Table table)
        {
            var properties = new Properties();

            foreach (var column in table.Columns)
            {
                if (column.DatabaseGenerated)
                {
                    _environment.WriteLine("WARNING: Elasticsearch doesn't support database-generated columns");
                    continue;
                }

                IProperty property = column.Type switch
                {
                    ColumnType.Boolean => new BooleanProperty { Index = column.Queryable },
                    ColumnType.Guid => new KeywordProperty { Index = column.Queryable },
                    ColumnType.String => new KeywordProperty { Index = column.Queryable },
                    ColumnType.Double => new DoubleNumberProperty { Index = column.Queryable },
                    ColumnType.Integer => new IntegerNumberProperty { Index = column.Queryable },
                    ColumnType.Text => new TextProperty { Index = column.Queryable },
                    ColumnType.DateTime => new DateProperty { Index = column.Queryable },
                    ColumnType.Vector => new DenseVectorProperty { Index = column.Queryable, Dims = column.Size },
                    _ => throw new InputArgumentException($"Unknown column type \"{column.Type}\"")
                };

                properties.Add(column.Name, property);
            }

            return properties;
        }

        private static Table NormalizeNames(Table table)
        {
            table.Name = table.Name.ToLower();
            return table;
        }
    }
}
