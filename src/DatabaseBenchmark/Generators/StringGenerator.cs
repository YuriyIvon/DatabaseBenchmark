using Bogus;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;

namespace DatabaseBenchmark.Generators
{
    public class StringGenerator : IGenerator
    {
        private readonly Randomizer _randomizer = new();
        private readonly StringGeneratorOptions _options;

        public object Current { get; private set; }

        public bool IsBounded => false;

        public StringGenerator(StringGeneratorOptions options)
        {
            _options = options;
        }

        public bool Next()
        {
            Current = _randomizer.String2(_options.MinLength, _options.MaxLength, _options.AllowedCharacters);

            return true;
        }
    }
}
