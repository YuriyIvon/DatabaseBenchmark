using Bogus;
using DatabaseBenchmark.Generators.Interfaces;

namespace DatabaseBenchmark.Generators
{
    public class NullGeneratorDecorator : IGenerator
    {
        private readonly Faker _faker;
        private readonly IGenerator _generator;
        private readonly float _nullProbability;

        public NullGeneratorDecorator(Faker faker, IGenerator generator, float nullProbability)
        {
            _faker = faker;
            _generator = generator;
            _nullProbability = nullProbability;
        }

        public object Generate()
            => _faker.Random.Bool(_nullProbability) ? null : _generator.Generate();
    }
}
