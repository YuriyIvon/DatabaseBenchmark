using Bogus;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;

namespace DatabaseBenchmark.Generators
{
    public class StringGenerator : IGenerator
    {
        private readonly Faker _faker;
        private readonly StringGeneratorOptions _options;

        public StringGenerator(Faker faker, StringGeneratorOptions options)
        {
            _faker = faker;
            _options = options;
        }

        public object Generate() =>
            _faker.Random.String2(_options.MinLength, _options.MaxLength, _options.AllowedCharacters);
    }
}
