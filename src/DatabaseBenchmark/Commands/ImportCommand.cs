using DatabaseBenchmark.Commands.Interfaces;
using DatabaseBenchmark.Commands.Options;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases;
using DatabaseBenchmark.DataSources;
using DatabaseBenchmark.Model;

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

            var pluginRepository = !string.IsNullOrEmpty(options.PluginsFilePath)
                ? new Plugins.PluginRepository(options.PluginsFilePath)
                : null;

            var dataSourceFactory = new DataSourceFactory(database, databaseFactory, _optionsProvider, pluginRepository);
            var baseDataSource = dataSourceFactory.Create(options.DataSourceType, options.DataSourceFilePath);
            using var dataSource = new DataSourceDecorator(baseDataSource)
                .MaxRows(options.DataSourceMaxRows)
                .Mapping(options.MappingFilePath)
                .TypedColumns(table.Columns, options.DataSourceCulture)
                .DataSource;

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
