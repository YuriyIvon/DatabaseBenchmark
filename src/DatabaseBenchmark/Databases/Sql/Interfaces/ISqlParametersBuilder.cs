using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.Sql.Interfaces
{
    public interface ISqlParametersBuilder
    {
        public IEnumerable<SqlQueryParameter> Parameters { get; }

        string Append(object value, ColumnType type, bool array);

        void Reset();
    }
}
