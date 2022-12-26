using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.Sql
{
    public class SqlParametersBuilder : ISqlParametersBuilder
    {
        private readonly List<SqlQueryParameter> _parameters = new();

        private int _counter = 0;

        public char Prefix { get; }

        public IEnumerable<SqlQueryParameter> Parameters => _parameters;

        public SqlParametersBuilder(char prefix = '@')
        {
            Prefix = prefix;
        }

        public string Append(object value, ColumnType type)
        {
            string name = $"p{_counter}";
            _counter++;

            var parameter = new SqlQueryParameter(Prefix, name, value, type);
            _parameters.Add(parameter);

            return Prefix + name;
        }

        public void Reset()
        {
            _counter = 0;
            _parameters.Clear();
        }
    }
}
