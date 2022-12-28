using DatabaseBenchmark.Databases.Common.Interfaces;
using System.Data;

namespace DatabaseBenchmark.Databases.MonetDb
{
    public class MonetDbTransactionProvider : ITransactionProvider
    {
        private readonly IDbConnection _connection;

        public MonetDbTransactionProvider(IDbConnection connection)
        {
            _connection = connection;
        }

        public ITransaction Begin()
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            return new MonetDbTransaction(_connection.BeginTransaction());
        }
    }
}
