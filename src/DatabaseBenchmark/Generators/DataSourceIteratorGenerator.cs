using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;

namespace DatabaseBenchmark.Generators
{
    public sealed class DataSourceIteratorGenerator : IGenerator, IDisposable
    {
        private readonly DataSourceIteratorGeneratorOptions _options;
        private readonly IDataSource _dataSource;
        private readonly bool _skipNavigation;

        public object Current { get; private set; }

        public DataSourceIteratorGenerator(DataSourceIteratorGeneratorOptions options, IDataSource dataSource, bool skipNavigation)
        {
            _options = options;
            _dataSource = dataSource;
            _skipNavigation = skipNavigation;
        }

        public bool Next()
        {
            if (_skipNavigation || _dataSource.Read())
            {
                Current = _dataSource.GetValue(_options.ColumnName);

                return true;
            }
            else
            {
                return false;
            }
        }

        public void Dispose()
        {
            if (!_skipNavigation)
            {
                _dataSource.Dispose();
            }
        }
    }
}
