using DatabaseBenchmark.Databases.Interfaces;
using System.Data;

namespace DatabaseBenchmark.Databases.Sql
{
    public sealed class SqlPreparedQuery : IPreparedQuery
    {
        private readonly IDbCommand _command;

        private SqlQueryResults _results;

        public IDictionary<string, double> CustomMetrics => null;

        public IQueryResults Results => _results;

        public SqlPreparedQuery(IDbCommand command)
        {
            _command = command;
        }

        public int Execute()
        {
            var reader = _command.ExecuteReader();
            _results = new SqlQueryResults(reader);

            return 0;
        }

        public void Dispose()
        {
            _results?.Dispose();
            _command.Dispose();
        }
    }
}
