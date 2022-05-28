using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.DataSources.Interfaces
{
    public interface IDataSourceFactory
    {
        IDataSource Create(string type, string filePath, Table table);
    }
}
