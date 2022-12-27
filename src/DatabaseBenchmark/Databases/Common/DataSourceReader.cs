using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.Common
{
    public class DataSourceReader : IDataSourceReader
    {
        private readonly object _syncObject = new();
        private readonly IDataSource _source;

        public DataSourceReader(IDataSource source)
        {
            _source = source;
        }

        public bool ReadDictionary(IEnumerable<Column> columns, out IDictionary<string, object> values)
        {
            lock (_syncObject)
            {
                if (!_source.Read())
                {
                    values = null;
                    return false;
                }

                values = columns
                    .Where(c => !c.DatabaseGenerated)
                    .ToDictionary(
                        c => c.Name,
                        c => _source.GetValue(c.GetNativeType(), c.Name));

                return true;
            }
        }

        public bool ReadArray(IEnumerable<Column> columns, out object[] values)
        {
            lock (_syncObject)
            {
                if (!_source.Read())
                {
                    values = null;
                    return false;
                }

                values = columns
                    .Where(c => !c.DatabaseGenerated)
                    .Select(c => _source.GetValue(c.GetNativeType(), c.Name))
                    .ToArray();

                return true;
            }
        }
    }
}
