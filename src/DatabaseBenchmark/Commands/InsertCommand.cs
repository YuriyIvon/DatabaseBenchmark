using DatabaseBenchmark.Commands.Interfaces;
using DatabaseBenchmark.Commands.Options;
using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases;
using DatabaseBenchmark.DataSources;
using DatabaseBenchmark.DataSources.Mapping;
using DatabaseBenchmark.Model;
using DatabaseBenchmark.Reporting;
using DatabaseBenchmark.Utils;

namespace DatabaseBenchmark.Commands
{
    public class InsertCommand : ICommand
    {
        private readonly IOptionsProvider _optionsProvider;

        private IExecutionEnvironment _environment;

        public InsertCommand(IOptionsProvider optionsProvider)
        {
            _optionsProvider = optionsProvider;
        }

        public void Execute()
        {
            var options = _optionsProvider.GetOptions<InsertCommandOptions>();

            _environment = new ExecutionEnvironment(options.TraceQueries);
            var metricsCollector = new MetricsCollector();
            var resultsBuilder = new ResultsBuilder(options.ReportColumns, options.ReportCustomMetricColumns);
            var benchmark = new QueryBenchmark(_environment, metricsCollector);
            var databaseFactory = new DatabaseFactory(_environment, _optionsProvider);
            var database = databaseFactory.Create(options.DatabaseType, options.ConnectionString);
            var table = JsonUtils.DeserializeFile<Table>(options.TableFilePath);

            if (!string.IsNullOrEmpty(options.TableName))
            {
                table.Name = options.TableName;
            }

            var dataSourceFactory = new DataSourceFactory(databaseFactory, _optionsProvider);
            var baseDataSource = dataSourceFactory.Create(options.DataSourceType, options.DataSourceFilePath);
            using var dataSource = !string.IsNullOrEmpty(options.MappingFilePath)
                ? new MappingDataSource(baseDataSource, JsonUtils.DeserializeFile<ColumnMappingCollection>(options.MappingFilePath))
                : baseDataSource;

            var executorFactory = database.CreateInsertExecutorFactory(table, dataSource, options.BatchSize);
            benchmark.Benchmark(executorFactory, options);

            ReportUtils.PrintReport(resultsBuilder, metricsCollector, options, _environment);
        }
    }
}
