using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.Common
{
    public class DummyGeneratorFactory : IGeneratorFactory
    {
        public IGenerator Create(IGeneratorOptions options)
        {
            throw new NotSupportedException("Data generator is not supported in this context");
        }
    }
}
