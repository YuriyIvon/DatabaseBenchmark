using DatabaseBenchmark.Commands.Interfaces;
using DatabaseBenchmark.Commands.Options;
using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases;
using DatabaseBenchmark.DataSources;
using DatabaseBenchmark.DataSources.Decorators;
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

            var dataSourceFactory = new DataSourceFactory(database, databaseFactory, _optionsProvider);
            var dataSource = dataSourceFactory.Create(options.DataSourceType, options.DataSourceFilePath);

            if (options.DataSourceMaxRows > 0)
            {
                dataSource = new DataSourceMaxRowsDecorator(dataSource, options.DataSourceMaxRows);
            }

            if (!string.IsNullOrEmpty(options.MappingFilePath))
            {
                dataSource = new DataSourceMappingDecorator(dataSource, JsonUtils.DeserializeFile<ColumnMappingCollection>(options.MappingFilePath));
            }

            //Is needed to guarantee the disposal of the data source
            using var dataSourceHolder = dataSource;

            using var importer = database.CreateDataImporter(table, dataSource, options.BatchSize);
            var result = importer.Import();

            if (!string.IsNullOrEmpty(options.PostScriptFilePath))
            {
                var script = File.ReadAllText(options.PostScriptFilePath);
                database.ExecuteScript(script);
            }

            Console.WriteLine($"Imported {result.Count} rows in {result.Duration / 1000.0} sec");

            if (result.CustomMetrics != null)
            {
                foreach (var metric in result.CustomMetrics)
                {
                    Console.WriteLine($"{metric.Key} = {metric.Value}");
                }
            }
        }
    }
}
