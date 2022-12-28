using DatabaseBenchmark.Databases.Common.Interfaces;
using System.Data;

namespace DatabaseBenchmark.Databases.Sql
{
    public class SqlTransactionProvider : ITransactionProvider
    {
        private readonly IDbConnection _connection;

        public SqlTransactionProvider(IDbConnection connection)
        {
            _connection = connection;
        }

        public ITransaction Begin()
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            return new SqlTransaction(_connection.BeginTransaction());
        }
    }
}
