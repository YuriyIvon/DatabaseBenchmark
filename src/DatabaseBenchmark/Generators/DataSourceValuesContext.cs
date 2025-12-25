using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Generators.Interfaces;

namespace DatabaseBenchmark.Generators
{
    public class DataSourceValuesContext : IGeneratedValuesContext
    {
        private readonly IDataSource _dataSource;

        public DataSourceValuesContext(IDataSource dataSource)
        {
            _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        }

        public object GetValue(string name) => _dataSource.GetValue(name);
    }
}
