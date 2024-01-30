using DatabaseBenchmark.Commands.Interfaces;
using DatabaseBenchmark.Commands.Options;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.DataSources;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Generators;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using DatabaseBenchmark.Model;
using DatabaseBenchmark.Reporting;

namespace DatabaseBenchmark.Commands
{
    public class QueryCommand : ICommand
    {
        private readonly IOptionsProvider _optionsProvider;

        private IExecutionEnvironment _environment;

        public QueryCommand(IOptionsProvider optionsProvider)
        {
            _optionsProvider = optionsProvider;
        }

        public void Execute()
        {
            var options = _optionsProvider.GetOptions<QueryCommandOptions>();

            _environment = new ExecutionEnvironment(options.TraceQueries, options.TraceResults);
            var metricsCollector = new MetricsCollector();
            var resultsBuilder = new ResultsBuilder(options.ReportColumns, options.ReportCustomMetricColumns);
            var benchmark = new QueryBenchmark(_environment, metricsCollector);

            var databaseFactory = new DatabaseFactory(_environment, _optionsProvider);
            var database = databaseFactory.Create(options.DatabaseType, options.ConnectionString);
            var table = JsonUtils.DeserializeFile<Table>(options.TableFilePath);
            var query = JsonUtils.DeserializeFile<Query>(options.QueryFilePath, new GeneratorOptionsConverter());

            if (!string.IsNullOrEmpty(options.TableName))
            {
                table.Name = options.TableName;
            }

            var executorFactory = database.CreateQueryExecutorFactory(table, query)
                .Customize<IGeneratorFactory, GeneratorFactory>()
                .Customize<IDatabaseFactory>(() => databaseFactory)
                .Customize<IOptionsProvider>(() => _optionsProvider)
                .Customize<IDataSourceFactory, DataSourceFactory>();

            benchmark.Benchmark(executorFactory, options);

            ReportUtils.PrintReport(resultsBuilder, metricsCollector, options, _environment);
        }
    }
}
