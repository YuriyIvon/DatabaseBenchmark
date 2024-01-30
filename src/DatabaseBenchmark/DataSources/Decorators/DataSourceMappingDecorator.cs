using DatabaseBenchmark.DataSources.Interfaces;

namespace DatabaseBenchmark.DataSources.Decorators
{
    public sealed class DataSourceMappingDecorator : IDataSource
    {
        private readonly IDataSource _baseDataSource;
        private readonly Dictionary<string, string> _mappings;

        public DataSourceMappingDecorator(IDataSource baseDataSource, ColumnMappingCollection mappings)
        {
            _baseDataSource = baseDataSource;
            _mappings = mappings.Columns.ToDictionary(m => m.TableColumnName, m => m.SourceColumnName);
        }

        public void Dispose() => _baseDataSource.Dispose();

        public object GetValue(string name) =>
            _baseDataSource.GetValue(_mappings.TryGetValue(name, out var sourceName) ? sourceName : name);

        public bool Read() => _baseDataSource.Read();
    }
}
