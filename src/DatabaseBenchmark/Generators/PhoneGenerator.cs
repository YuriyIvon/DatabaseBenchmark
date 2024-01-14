using Bogus;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using GeneratorKind = DatabaseBenchmark.Generators.Options.PhoneGeneratorOptions.GeneratorKind;

namespace DatabaseBenchmark.Generators
{
    public class PhoneGenerator : IGenerator
    {
        private readonly Faker _faker;
        private readonly PhoneGeneratorOptions _options;

        public object Current {  get; private set; }

        public PhoneGenerator(Faker faker, PhoneGeneratorOptions options)
        {
            _faker = faker;
            _options = options;
        }

        public bool Next()
        {
            Current = _options.Kind switch
            {
                GeneratorKind.PhoneNumber => _faker.Phone.PhoneNumber(),
                _ => throw new InputArgumentException($"Unknown phone generator kind \"{_options.Kind}\"")
            };

            return true;
        }
    }
}
