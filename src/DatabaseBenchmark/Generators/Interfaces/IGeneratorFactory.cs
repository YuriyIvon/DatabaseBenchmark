using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Generators.Interfaces
{
    public interface IGeneratorFactory
    {
        public IGenerator Create(IGeneratorOptions options);
    }
}
