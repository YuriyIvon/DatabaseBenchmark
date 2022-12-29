using DatabaseBenchmark.Databases.Sql.Interfaces;
using System.Data;

namespace DatabaseBenchmark.Databases.MonetDb
{
    public sealed class MonetDbTransaction : ISqlTransaction
    {
        private readonly IDbTransaction _transaction;

        //Since MonetDB driver doesn't support transaction assignment to ADO .NET commands
        public IDbTransaction Transaction => null;

        public MonetDbTransaction(IDbTransaction transaction)
        {
            _transaction = transaction;
        }

        public void Commit()
        {
            _transaction.Commit();
        }

        public void Rollback()
        {
            _transaction.Rollback();
        }

        public void Dispose()
        {
            _transaction.Dispose();
        }
    }
}
