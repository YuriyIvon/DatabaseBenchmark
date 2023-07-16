using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.Sql
{
    public class SqlParametersBuilder : ISqlParametersBuilder
    {
        private readonly char _prefix;
        private readonly bool _isOrdinal;
        private readonly List<SqlQueryParameter> _parameters = new();

        private int _counter = 0;

        public IEnumerable<SqlQueryParameter> Parameters => _parameters;

        public SqlParametersBuilder(char prefix = '@', bool isOrdinal = false)
        {
            _prefix = prefix;
            _isOrdinal = isOrdinal;
        }

        public string Append(object value, ColumnType type)
        {
            string name = $"p{_counter}";
            _counter++;

            var parameter = new SqlQueryParameter(_prefix, name, value, type);
            _parameters.Add(parameter);

            return _isOrdinal ? new string(_prefix, 1) : _prefix + name;
        }

        public void Reset()
        {
            _counter = 0;
            _parameters.Clear();
        }
    }
}
