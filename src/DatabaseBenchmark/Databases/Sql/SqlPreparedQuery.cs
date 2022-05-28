using DatabaseBenchmark.Databases.Interfaces;
using System.Data;

namespace DatabaseBenchmark.Databases.Sql
{
    public class SqlPreparedQuery : IPreparedQuery
    {
        private readonly IDbCommand _command;
        private IDataReader _reader;
        private Dictionary<string, int> _columnNames;

        public IEnumerable<string> ColumnNames => _columnNames.Keys;

        public IDictionary<string, double> CustomMetrics => null;

        public SqlPreparedQuery(IDbCommand command)
        {
            _command = command;
        }

        public void Execute()
        {
            _reader = _command.ExecuteReader();
            _columnNames = Enumerable.Range(0, _reader.FieldCount)
                .ToDictionary(i => _reader.GetName(i), i => i, StringComparer.OrdinalIgnoreCase);
        }

        public object GetValue(string name) => _reader.GetValue(_columnNames[name]);

        public bool Read() => _reader.Read();

        public void Dispose()
        {
            if (_reader != null)
            {
                _reader.Dispose();
            }
        }
    }
}
