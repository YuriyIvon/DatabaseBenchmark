namespace DatabaseBenchmark.DataSources.Interfaces
{
    public interface IDataSource : IDisposable
    {
        object GetValue(string name);

        bool Read();
    }
}
