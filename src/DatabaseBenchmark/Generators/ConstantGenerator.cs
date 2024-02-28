using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;

namespace DatabaseBenchmark.Generators
{
    public class ConstantGenerator : IGenerator
    {
        private readonly ConstantGeneratorOptions _options;

        public object Current => _options.Value;

        public ConstantGenerator(ConstantGeneratorOptions options)
        {
            _options = options;
        }

        public bool Next() => true;
    }
}
