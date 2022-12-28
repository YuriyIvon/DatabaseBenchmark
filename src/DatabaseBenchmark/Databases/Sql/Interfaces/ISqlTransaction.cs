using DatabaseBenchmark.Databases.Common.Interfaces;
using System.Data;

namespace DatabaseBenchmark.Databases.Sql.Interfaces
{
    public interface ISqlTransaction : ITransaction
    {
        IDbTransaction Transaction { get; }
    }
}
