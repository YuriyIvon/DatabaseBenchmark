using DatabaseBenchmark.Core.Interfaces;

namespace DatabaseBenchmark.Databases.Interfaces
{
    public interface IDatabaseFactory
    {
        IDatabase Create(string type, string connectionString);
    }
}
