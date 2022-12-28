using DatabaseBenchmark.Databases.Model;

namespace DatabaseBenchmark.Databases.Common.Interfaces
{
    public interface IDataImporter : IDisposable
    {
        ImportResult Import();
    }
}
