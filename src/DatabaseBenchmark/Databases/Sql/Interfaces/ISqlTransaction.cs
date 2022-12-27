using DatabaseBenchmark.Databases.Interfaces;
using System.Data;

namespace DatabaseBenchmark.Databases.Sql.Interfaces
{
    public interface ISqlTransaction : ITransaction
    {
        IDbTransaction Transaction { get; }
    }
}
