using Bogus;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;

namespace DatabaseBenchmark.Generators
{
    public class NullGenerator : IGenerator
    {
        private readonly Faker _faker;
        private readonly NullGeneratorOptions _options;
        private readonly IGenerator _sourceGenerator;

        public NullGenerator(Faker faker, NullGeneratorOptions options, IGenerator sourceGenerator)
        {
            _faker = faker;
            _options = options;
            _sourceGenerator = sourceGenerator;
        }

        public object Generate()
            => _faker.Random.Bool(_options.Weight) ? null : _sourceGenerator.Generate();
    }
}
