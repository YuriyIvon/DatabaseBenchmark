namespace DatabaseBenchmark.Core.Interfaces
{
    public interface IOptionsProvider
    {
        T GetOptions<T>() where T : new();
    }
}
