using DatabaseBenchmark.DataSources.Interfaces;

namespace DatabaseBenchmark.DataSources.Decorators
{
    public sealed class DataSourceMaxRowsDecorator : IDataSource
    {
        private readonly IDataSource _dataSource;

        private int _remainingRows;

        public DataSourceMaxRowsDecorator(IDataSource dataSource, int maxRows)
        {
            _dataSource = dataSource;
            _remainingRows = maxRows;
        }

        public object GetValue(string name) => _dataSource.GetValue(name);

        public bool Read()
        {
            if (_remainingRows > 0)
            {
                _remainingRows--;
                return _dataSource.Read();
            }
            else
            {
                return false;
            }
        }

        public void Dispose() => _dataSource.Dispose();
    }
}
