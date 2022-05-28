using DatabaseBenchmark.Commands.Interfaces;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;

namespace DatabaseBenchmark.Commands
{
    public class CommandFactory : ICommandFactory, IAllowedValuesProvider
    {
        private readonly Dictionary<string, Func<ICommand>> _factories;

        public IEnumerable<string> Options => _factories.Keys;

        public CommandFactory(IOptionsProvider optionsProvider)
        {
            _factories = new()
            {
                ["create"] = () => new CreateCommand(optionsProvider),
                ["import"] = () => new ImportCommand(optionsProvider),
                ["query"] = () =>  new QueryCommand(optionsProvider),
                ["query-scenario"] = () => new QueryScenarioCommand(optionsProvider),
                ["raw-query"] = () => new RawQueryCommand(optionsProvider),
                ["raw-query-scenario"] = () => new RawQueryScenarioCommand(optionsProvider)
            };
        }

        public ICommand Create(string commandName)
        {
            if (!_factories.TryGetValue(commandName, out var factory))
            {
                throw new InputArgumentException($"Unknown command \"{commandName}\"");
            }

            return factory();
        }
    }
}
