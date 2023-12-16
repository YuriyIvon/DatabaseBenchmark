using DatabaseBenchmark.Commands.Interfaces;
using DatabaseBenchmark.Commands.Options;
using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases;
using DatabaseBenchmark.DataSources;
using DatabaseBenchmark.DataSources.Decorators;
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
            var dataSource = dataSourceFactory.Create(options.DataSourceType, options.DataSourceFilePath);

            if (options.DataSourceMaxRows > 0)
            {
                dataSource = new DataSourceMaxRowsDecorator(dataSource, options.DataSourceMaxRows);
            }

            if(!string.IsNullOrEmpty(options.MappingFilePath))
            {
                dataSource = new DataSourceMappingDecorator(dataSource, JsonUtils.DeserializeFile<ColumnMappingCollection>(options.MappingFilePath));
            }

            //Is needed to guarantee the disposal of the data source
            using var dataSourceHolder = dataSource;

            var executorFactory = database.CreateInsertExecutorFactory(table, dataSource, options.BatchSize);
            benchmark.Benchmark(executorFactory, options);

            ReportUtils.PrintReport(resultsBuilder, metricsCollector, options, _environment);
        }
    }
}
