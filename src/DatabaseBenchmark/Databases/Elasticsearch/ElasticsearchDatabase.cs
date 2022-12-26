using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Databases.Model;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using Nest;
using System.Diagnostics;
using System.Text;
using RawQuery = DatabaseBenchmark.Model.RawQuery;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public class ElasticsearchDatabase : IDatabase
    {
        private const int DefaultImportBatchSize = 500;

        private readonly string _connectionString;
        private readonly IExecutionEnvironment _environment;

        public ElasticsearchDatabase(string connectionString, IExecutionEnvironment environment)
        {
            _connectionString = connectionString;
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

        public ImportResult ImportData(Table table, IDataSource source, int batchSize)
        {
            table = NormalizeNames(table);

            if (batchSize <= 0)
            {
                batchSize = DefaultImportBatchSize;
            }

            var client = CreateClient();

            var buffer = new List<object>();

            var stopwatch = Stopwatch.StartNew();
            var progressReporter = new ImportProgressReporter(_environment);

            while (source.Read())
            {
                var document = table.Columns
                    .Where(c => !c.DatabaseGenerated)
                    .ToDictionary(
                        c => c.Name,
                        c => source.GetValue(c.GetNativeType(), c.Name));

                buffer.Add(document);

                if (buffer.Count >= batchSize)
                {
                    client.IndexMany(buffer, table.Name);
                    progressReporter.Increment(buffer.Count);
                    buffer.Clear();
                }
            }

            if (buffer.Count > 0)
            {
                client.IndexMany(buffer, table.Name);
            }

            client.Indices.Refresh(table.Name);

            stopwatch.Stop();

            var stats = client.Indices.Stats(table.Name);

            var importResult = new ImportResult(stats.Stats.Primaries.Documents.Count, stopwatch.ElapsedMilliseconds);
            importResult.AddMetric(Common.Metrics.TotalStorageBytes, (long)stats.Stats.Total.Store.SizeInBytes);

            return importResult;
        }

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query) =>
            new ElasticsearchQueryExecutorFactory(CreateClient, NormalizeNames(table), query);

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            new ElasticsearchRawQueryExecutorFactory(CreateClient, query);

        public IQueryExecutorFactory CreateInsertExecutorFactory(Table table, IDataSource source) =>
            new ElasticsearchInsertExecutorFactory(CreateClient, table, source);

        private ElasticClient CreateClient()
        {
            var connectionSettings = new ConnectionSettings(new Uri(_connectionString))
                .ThrowExceptions();

            if (_environment.TraceQueries || _environment.TraceResults)
            {
                connectionSettings.EnableDebugMode(response =>
                {
                    if (_environment.TraceQueries)
                    {
                        _environment.WriteLine(Encoding.UTF8.GetString(response.RequestBodyInBytes));
                    }

                    if (_environment.TraceResults)
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
