namespace DatabaseBenchmark.Generators.Interfaces
{
    public interface IGeneratorFactory
    {
        public IGenerator Create(GeneratorType type, IGeneratorOptions options);
    }
}
