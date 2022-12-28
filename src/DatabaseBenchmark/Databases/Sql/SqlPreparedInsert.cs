using DatabaseBenchmark.Databases.Common.Interfaces;
using System.Data;

namespace DatabaseBenchmark.Databases.Sql
{
    public sealed class SqlPreparedInsert : IPreparedQuery
    {
        private readonly IDbCommand _command;

        public IDictionary<string, double> CustomMetrics => null;

        public IQueryResults Results => null;

        public SqlPreparedInsert(IDbCommand command)
        {
            _command = command;
        }

        public int Execute() => _command.ExecuteNonQuery();

        public void Dispose() => _command.Dispose();
    }
}
