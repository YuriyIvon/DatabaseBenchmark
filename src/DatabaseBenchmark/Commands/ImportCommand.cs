using DatabaseBenchmark.Commands.Interfaces;
using DatabaseBenchmark.Commands.Options;
using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases;
using DatabaseBenchmark.DataSources;
using DatabaseBenchmark.Model;
using DatabaseBenchmark.Utils;

namespace DatabaseBenchmark.Commands
{
    public class ImportCommand : ICommand
    {
        private readonly IOptionsProvider _optionsProvider;

        public ImportCommand(IOptionsProvider optionsProvider)
        {
            _optionsProvider = optionsProvider;
        }

        public void Execute()
        {
            var options = _optionsProvider.GetOptions<ImportCommandOptions>();

            var environment = new ExecutionEnvironment(options.TraceQueries);
            var databaseFactory = new DatabaseFactory(environment, _optionsProvider);
            var database = databaseFactory.Create(options.DatabaseType, options.ConnectionString);
            var table = JsonUtils.DeserializeFile<Table>(options.TableFilePath);

            if (!string.IsNullOrEmpty(options.TableName))
            {
                table.Name = options.TableName;
            }

            var dataSourceFactory = new DataSourceFactory(databaseFactory, _optionsProvider);
            using var dataSource = dataSourceFactory.Create(options.DataSourceType, options.DataSourceFilePath, table);

            var result = database.ImportData(table, dataSource, options.ImportBatchSize);

            Console.WriteLine($"Imported {result.Count} rows in {result.Duration / 1000.0} sec");
            
            foreach (var metric in result.CustomMetrics)
            {
                Console.WriteLine($"{metric.Key} = {metric.Value}");
            }
        }
    }
}
