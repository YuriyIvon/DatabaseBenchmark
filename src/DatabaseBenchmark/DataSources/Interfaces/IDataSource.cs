namespace DatabaseBenchmark.DataSources.Interfaces
{
    public interface IDataSource : IDisposable
    {
        object GetValue(Type type, string name);

        bool Read();
    }
}
