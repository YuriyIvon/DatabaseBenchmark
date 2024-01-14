namespace DatabaseBenchmark.Generators.Interfaces
{
    public interface ICollectionGenerator
    {
        IEnumerable<object> CurrentCollection { get; }

        bool NextCollection(int length);
    }
}
