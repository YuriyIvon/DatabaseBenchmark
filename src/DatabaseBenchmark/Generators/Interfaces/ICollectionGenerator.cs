namespace DatabaseBenchmark.Generators.Interfaces
{
    public interface ICollectionGenerator
    {
        IEnumerable<object> GenerateCollection(int length);
    }
}
