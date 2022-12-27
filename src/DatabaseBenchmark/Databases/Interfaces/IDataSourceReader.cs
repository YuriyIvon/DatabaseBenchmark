using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.Interfaces
{
    public interface IDataSourceReader
    {
        bool ReadDictionary(IEnumerable<Column> columns, out IDictionary<string, object> values);

        bool ReadArray(IEnumerable<Column> columns, out object[] values);
    }
}
