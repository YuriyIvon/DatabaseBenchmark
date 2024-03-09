using Bogus;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using Fare;
using static DatabaseBenchmark.Generators.Options.PatternGeneratorOptions;

namespace DatabaseBenchmark.Generators
{
    public class PatternGenerator : IGenerator
    {
        private readonly PatternGeneratorOptions _options;

        private Randomizer _randomizer;
        private Xeger _regexGenerator;

        public object Current { get; private set; }

        public bool IsBounded => false;

        private Randomizer Randomizer
        {
            get
            {
                _randomizer ??= new Randomizer();
                return _randomizer;
            }
        }

        private Xeger RegexGenerator
        {
            get
            {
                _regexGenerator ??= new Xeger(_options.Pattern);
                return _regexGenerator;
            }
        }

        public PatternGenerator(PatternGeneratorOptions options)
        {
            _options = options;
        }

        public bool Next()
        {
            if (string.IsNullOrEmpty(_options.Pattern))
            {
                throw new InputArgumentException($"Pattern is not specified for the pattern generator");
            }

            Current = _options.Kind switch
            {
                GeneratorKind.Simple => Randomizer.Replace(_options.Pattern),
                GeneratorKind.Regex => RegexGenerator.Generate(),
                _ => throw new InputArgumentException($"Unknown pattern generator kind \"{_options.Kind}\"")
            };

            return true;
        }
    }
}
