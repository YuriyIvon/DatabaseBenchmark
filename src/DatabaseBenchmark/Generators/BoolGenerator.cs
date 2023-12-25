using Bogus;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;

namespace DatabaseBenchmark.Generators
{
    public class BoolGenerator : IGenerator
    {
        private readonly Faker _faker;
        private readonly BooleanGeneratorOptions _options;

        public BoolGenerator(Faker faker, BooleanGeneratorOptions options)
        {
            _faker = faker;
            _options = options;
        }

        public object Generate() => _faker.Random.Bool(_options.Weight);
    }
}
