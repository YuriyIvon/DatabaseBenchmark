using Bogus;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;

namespace DatabaseBenchmark.Generators
{
    public class BooleanGenerator : IGenerator
    {
        private readonly Randomizer _randomizer = new();
        private readonly BooleanGeneratorOptions _options;

        public object Current { get; private set; }

        public bool IsBounded => false;

        public BooleanGenerator(BooleanGeneratorOptions options)
        {
            _options = options;
        }

        public bool Next()
        {
            Current = _options.Weight != null
                ? _randomizer.Bool(_options.Weight.Value)
                : _randomizer.Bool();

            return true;
        }
    }
}
