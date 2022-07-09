using DatabaseBenchmark.Commands.Interfaces;
using DatabaseBenchmark.Commands.Options;
using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases;
using DatabaseBenchmark.Model;
using DatabaseBenchmark.Utils;

namespace DatabaseBenchmark.Commands
{
    public class CreateCommand : ICommand
    {
        private readonly IOptionsProvider _optionsProvider;

        public CreateCommand(IOptionsProvider optionsProvider)
        {
            _optionsProvider = optionsProvider;
        }

        public void Execute()
        {
            var options = _optionsProvider.GetOptions<CreateCommandOptions>();

            var environment = new ExecutionEnvironment(options.TraceQueries);
            var databaseFactory = new DatabaseFactory(environment, _optionsProvider);
            var database = databaseFactory.Create(options.DatabaseType, options.ConnectionString);
            var table = JsonUtils.DeserializeFile<Table>(options.TableFilePath);

            if (!string.IsNullOrEmpty(options.TableName))
            {
                table.Name = options.TableName;
            }

            database.CreateTable(table, options.DropExisting);
        }
    }
}
