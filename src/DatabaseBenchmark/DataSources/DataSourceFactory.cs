using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.DataSources.Csv;
using DatabaseBenchmark.DataSources.Database;
using DatabaseBenchmark.DataSources.Generator;
using DatabaseBenchmark.DataSources.Parquet;
using DatabaseBenchmark.DataSources.Interfaces;

namespace DatabaseBenchmark.DataSources
{
    public class DataSourceFactory : IDataSourceFactory, IAllowedValuesProvider
    {
        private readonly Dictionary<string, Func<string, IDataSource>> _factories;
        public IEnumerable<string> Options => _factories.Keys;

        //TODO: find a way to avoid the direct database project dependency
        public DataSourceFactory(IDatabase currentDatabase, IDatabaseFactory databaseFactory, IOptionsProvider optionsProvider)
        {
            _factories = new()
            {
                ["Csv"] = filePath => new CsvDataSource(filePath, optionsProvider),
                ["Parquet"] = filePath => new ParquetDataSource(filePath),
                ["Database"] = filePath => new DatabaseDataSource(filePath, databaseFactory),
                ["Generator"] = filePath => new GeneratorDataSource(filePath, this, currentDatabase)
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
