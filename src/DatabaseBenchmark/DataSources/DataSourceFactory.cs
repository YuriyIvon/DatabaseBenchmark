using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.DataSources.Csv;
using DatabaseBenchmark.DataSources.Database;
using DatabaseBenchmark.DataSources.Interfaces;

namespace DatabaseBenchmark.DataSources
{
    public class DataSourceFactory : IDataSourceFactory, IAllowedValuesProvider
    {
        private readonly Dictionary<string, Func<string, IDataSource>> _factories;
        public IEnumerable<string> Options => _factories.Keys;

        //TODO: find a way to avoid the direct database project dependency
        public DataSourceFactory(IDatabaseFactory databaseFactory, IOptionsProvider optionsProvider)
        {
            _factories = new()
            {
                ["Csv"] = filePath => new CsvDataSource(filePath, optionsProvider),
                ["Database"] = filePath => new DatabaseDataSource(filePath, databaseFactory)
            };
        }

        public IDataSource Create(string type, string filePath)
        {
            if (_factories.TryGetValue(type, out var factory))
            {
                return factory(filePath);
            }

            throw new InputArgumentException($"Unknown data source type \"{type}\"");
        }
    }
}
