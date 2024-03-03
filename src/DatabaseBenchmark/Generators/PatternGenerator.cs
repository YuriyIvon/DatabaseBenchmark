using Bogus;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using static DatabaseBenchmark.Generators.Options.PatternGeneratorOptions;

namespace DatabaseBenchmark.Generators
{
    public class PatternGenerator : IGenerator
    {
        private readonly Randomizer _randomizer = new();
        private readonly PatternGeneratorOptions _options;

        public object Current { get; private set; }

        public bool IsBounded => false;

        public PatternGenerator(PatternGeneratorOptions options)
        {
            _options = options;
        }

        public bool Next()
        {
            if (string.IsNullOrEmpty(_options.Pattern))
            {
                new InputArgumentException($"Pattern is not specified for the pattern generator");
            }

            Current = _options.Kind switch
            {
                GeneratorKind.Simple => _randomizer.Replace(_options.Pattern),
                _ => throw new InputArgumentException($"Unknown pattern generator kind \"{_options.Kind}\"")
            };

            return true;
        }
    }
}
