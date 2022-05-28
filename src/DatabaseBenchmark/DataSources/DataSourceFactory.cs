using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.DataSources.Csv;
using DatabaseBenchmark.DataSources.Database;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.DataSources
{
    public class DataSourceFactory : IDataSourceFactory, IAllowedValuesProvider
    {
        private readonly Dictionary<string, Func<string, Table, IDataSource>> _factories;
        public IEnumerable<string> Options => _factories.Keys;

        //TODO: find a way to avoid the direct database project dependency
        public DataSourceFactory(IDatabaseFactory databaseFactory)
        {
            _factories = new()
            {
                ["Csv"] = (filePath, table) => new CsvDataSource(filePath, table),
                ["Database"] = (filePath, table) => new DatabaseDataSource(filePath, databaseFactory)
            };
        }

        public IDataSource Create(string type, string filePath, Table table)
        {
            if (_factories.TryGetValue(type, out var factory))
            {
                return factory(filePath, table);
            }

            throw new InputArgumentException($"Unknown data source type \"{type}\"");
        }
    }
}
