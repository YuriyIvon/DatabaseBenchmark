using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.AzureSearch.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using RawQuery = DatabaseBenchmark.Model.RawQuery;

namespace DatabaseBenchmark.Databases.AzureSearch
{
    public class AzureSearchDatabase : IDatabase
    {
        private const int DefaultImportBatchSize = 500;

        private const string EndpointConnectionStringProperty = "Endpoint";
        private const string ApiKeyConnectionStringProperty = "ApiKey";
        private const string VectorProfileName = "vector-profile";
        private const string VectorAlgorithmConfigurationName = "algorithm-config";

        private readonly IExecutionEnvironment _environment;

        public string ConnectionString { get; }

        public AzureSearchDatabase(string connectionString, IExecutionEnvironment environment)
        {
            ConnectionString = connectionString;
            _environment = environment;
        }

        public void CreateTable(Table table, bool dropExisting)
        {
            table.Name = NormalizeTableName(table.Name);

            var indexClient = CreateIndexClient();

            if (dropExisting)
            {
                try
                {
                    if (indexClient.GetIndex(table.Name) != null)
                    {
                        indexClient.DeleteIndex(table.Name);
                    }
                }
                catch (RequestFailedException ex) when (ex.Status == 404)
                {
                    // Index doesn't exist, which is fine
                }
            }

            var indexDefinition = BuildIndexDefinition(table);
            indexClient.CreateOrUpdateIndex(indexDefinition);
        }

        public IDataImporter CreateDataImporter(Table table, IDataSource source, int batchSize)
        {
            table.Name = NormalizeTableName(table.Name);

            return new DataImporterBuilder(table, source, batchSize, DefaultImportBatchSize)
                .TransactionProvider<NoTransactionProvider>()
                .InsertBuilder<IAzureSearchInsertBuilder, AzureSearchInsertBuilder>()
                .InsertExecutor<AzureSearchInsertExecutor>()
                .DataMetricsProvider<AzureSearchDataMetricsProvider>()
                .ProgressReporter<ImportProgressReporter>()
                .Environment(_environment)
                .Customize((container, lifestyle) =>
                {
                    container.Register(() => CreateSearchClient(table.Name), lifestyle);
                })
                .Build();
        }

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query)
        {
            table.Name = NormalizeTableName(table.Name);
            return new AzureSearchQueryExecutorFactory(this, () => CreateSearchClient(table.Name), table, query, _environment);
        }

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            new AzureSearchRawQueryExecutorFactory(this, () => CreateSearchClient(NormalizeTableName(query.TableName)), query, _environment);

        public IQueryExecutorFactory CreateInsertExecutorFactory(Table table, IDataSource source, int batchSize)
        {
            table.Name = NormalizeTableName(table.Name);
            return new AzureSearchInsertExecutorFactory(() => CreateSearchClient(table.Name), table, source, batchSize);
        }

        public void ExecuteScript(string script) => throw new InputArgumentException("Custom scripts are not supported for Azure Search");

        private SearchIndexClient CreateIndexClient()
        {
            var connectionParameters = ConnectionStringParser.Parse(
                ConnectionString,
                EndpointConnectionStringProperty,
                ApiKeyConnectionStringProperty);

            var endpoint = new Uri(connectionParameters[EndpointConnectionStringProperty]);
            var credential = new AzureKeyCredential(connectionParameters[ApiKeyConnectionStringProperty]);
            var client = new SearchIndexClient(endpoint, credential);

            return client;
        }

        private SearchClient CreateSearchClient(string indexName)
        {
            var connectionParameters = ConnectionStringParser.Parse(
                ConnectionString,
                EndpointConnectionStringProperty,
                ApiKeyConnectionStringProperty);

            var endpoint = new Uri(connectionParameters[EndpointConnectionStringProperty]);
            var credential = new AzureKeyCredential(connectionParameters[ApiKeyConnectionStringProperty]);
            var client = new SearchClient(endpoint, indexName, credential);

            return client;
        }

        private SearchIndex BuildIndexDefinition(Table table)
        {
            var fields = new List<SearchField>();

            if (!table.Columns.Any(c => c.PrimaryKey))
            {
                throw new InputArgumentException("Azure Search requires a primary key to be defined");
            }

            if (table.Columns.Any(c => c.DatabaseGenerated))
            {
                _environment.WriteLine("WARNING: Azure Search doesn't support database-generated columns");
            }

            VectorSearch vectorSearch = null;

            foreach (var column in table.Columns)
            {
                SearchField field;

                if (column.Type == ColumnType.Text)
                {
                    field = new SearchableField(column.Name)
                    {
                        IsKey = column.PrimaryKey,
                        IsFilterable = column.Queryable,
                        IsSortable = false,
                        IsFacetable = false
                    };
                }
                else if (column.Type == ColumnType.Vector)
                {
                    vectorSearch ??= new VectorSearch
                    {
                        Algorithms = { new HnswAlgorithmConfiguration(VectorAlgorithmConfigurationName) },
                        Profiles = { new VectorSearchProfile(VectorProfileName, VectorAlgorithmConfigurationName) }
                    };

                    field = new VectorSearchField(column.Name, column.Size.Value, VectorProfileName);
                }
                else
                {
                    var fieldType = column.Type switch
                    {
                        ColumnType.Boolean => SearchFieldDataType.Boolean,
                        ColumnType.Guid or ColumnType.String => SearchFieldDataType.String,
                        ColumnType.Integer => SearchFieldDataType.Int32,
                        ColumnType.Long => SearchFieldDataType.Int64,
                        ColumnType.Double => SearchFieldDataType.Double,
                        ColumnType.DateTime => SearchFieldDataType.DateTimeOffset,
                        _ => throw new InputArgumentException($"Unknown column type \"{column.Type}\"")
                    };

                    if (column.Array)
                    {
                        fieldType = SearchFieldDataType.Collection(fieldType);
                    }

                    field = new SimpleField(column.Name, fieldType)
                    {
                        IsKey = column.PrimaryKey,
                        IsFacetable = column.Queryable,
                        IsSortable = column.Queryable && !column.Array,
                        IsFilterable = column.Queryable
                    };
                }

                fields.Add(field);
            }

            var indexDefinition = new SearchIndex(table.Name, fields)
            {
                VectorSearch = vectorSearch
            };

            return indexDefinition;
        }

        private static string NormalizeTableName(string tableName) => tableName.ToLower();
    }
}