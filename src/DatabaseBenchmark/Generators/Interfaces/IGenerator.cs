namespace DatabaseBenchmark.Generators.Interfaces
{
    public interface IGenerator
    {
        object Current { get; }

        bool Next();

        bool IsBounded { get; }
    }
}
