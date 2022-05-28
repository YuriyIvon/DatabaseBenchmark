namespace DatabaseBenchmark.Core.Interfaces
{
    public interface IAllowedValuesProvider
    {
        IEnumerable<string> Options { get; }
    }
}
