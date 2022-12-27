using DatabaseBenchmark.Databases.Sql.Interfaces;
using System.Data;

namespace DatabaseBenchmark.Databases.Sql
{
    public sealed class SqlTransaction : ISqlTransaction
    {
        public IDbTransaction Transaction { get; }

        public SqlTransaction(IDbTransaction transaction)
        {
            Transaction = transaction;
        }

        public void Commit()
        {
            Transaction.Commit();
        }

        public void Rollback()
        {
            Transaction.Rollback();
        }

        public void Dispose()
        {
            Transaction.Dispose();
        }
    }
}
