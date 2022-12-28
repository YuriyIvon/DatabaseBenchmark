using DatabaseBenchmark.Core.Interfaces;

namespace DatabaseBenchmark.Databases.Common.Interfaces
{
    public interface IDatabaseFactory
    {
        IDatabase Create(string type, string connectionString);
    }
}
