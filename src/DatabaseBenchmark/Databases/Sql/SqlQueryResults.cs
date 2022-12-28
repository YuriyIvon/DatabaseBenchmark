using DatabaseBenchmark.Databases.Common.Interfaces;
using System.Data;

namespace DatabaseBenchmark.Databases.Sql
{
    public sealed class SqlQueryResults : IQueryResults, IDisposable
    {
        private readonly IDataReader _reader;
        private readonly Dictionary<string, int> _columnNames;

        public IEnumerable<string> ColumnNames => _columnNames.Keys;

        public SqlQueryResults(IDataReader reader)
        {
            _reader = reader;
            _columnNames = Enumerable.Range(0, _reader.FieldCount)
                .ToDictionary(i => _reader.GetName(i), i => i, StringComparer.OrdinalIgnoreCase);
        }

        public object GetValue(string name)
        {
            var value = _reader.GetValue(_columnNames[name]);
            return value != DBNull.Value ? value : null;
        }

        public bool Read() => _reader.Read();

        public void Dispose() => _reader?.Dispose();
    }
}
