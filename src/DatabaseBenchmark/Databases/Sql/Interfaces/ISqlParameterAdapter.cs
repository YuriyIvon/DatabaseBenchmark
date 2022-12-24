using System.Data;

namespace DatabaseBenchmark.Databases.Sql.Interfaces
{
    public interface ISqlParameterAdapter
    {
        void Populate(SqlQueryParameter source, IDbDataParameter target);
    }
}
