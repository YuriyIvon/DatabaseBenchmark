using Bogus.DataSets;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using GeneratorKind = DatabaseBenchmark.Generators.Options.PhoneGeneratorOptions.GeneratorKind;

namespace DatabaseBenchmark.Generators
{
    public class PhoneGenerator : IGenerator
    {
        private readonly PhoneNumbers _phoneFaker;
        private readonly PhoneGeneratorOptions _options;

        public object Current {  get; private set; }

        public PhoneGenerator(PhoneGeneratorOptions options)
        {
            _options = options;
            _phoneFaker = string.IsNullOrEmpty(options.Locale) ? new PhoneNumbers() : new PhoneNumbers(locale: options.Locale);
        }

        public bool Next()
        {
            Current = _options.Kind switch
            {
                GeneratorKind.PhoneNumber => _phoneFaker.PhoneNumber(),
                _ => throw new InputArgumentException($"Unknown phone generator kind \"{_options.Kind}\"")
            };

            return true;
        }
    }
}
