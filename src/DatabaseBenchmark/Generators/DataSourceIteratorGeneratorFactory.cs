using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Generators.Options;

namespace DatabaseBenchmark.Generators
{
    public class DataSourceIteratorGeneratorFactory
    {
        private readonly IDataSourceFactory _dataSourceFactory;
        private readonly Dictionary<(string, string), IDataSource> _dataSourceCache = [];

        public DataSourceIteratorGeneratorFactory(IDataSourceFactory dataSourceFactory)
        {
            _dataSourceFactory = dataSourceFactory;
        }

        public DataSourceIteratorGenerator Create(DataSourceIteratorGeneratorOptions options)
        {
            if (_dataSourceCache.TryGetValue((options.DataSourceType, options.DataSourceFilePath), out var dataSource))
            {
                //All iterators created for the already cached data source should not move through the dataset on calls to Next()
                return new DataSourceIteratorGenerator(options, dataSource, true);
            }
            else
            {
                dataSource = _dataSourceFactory.Create(options.DataSourceType, options.DataSourceFilePath);
                _dataSourceCache.Add((options.DataSourceType, options.DataSourceFilePath), dataSource);

                return new DataSourceIteratorGenerator(options, dataSource, false);
            }
        }
    }
}
