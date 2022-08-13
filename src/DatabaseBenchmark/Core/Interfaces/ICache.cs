namespace DatabaseBenchmark.Core.Interfaces
{
    public interface ICache
    {
        T GetOrRead<T>(string key, Func<T> reader);
    }
}
