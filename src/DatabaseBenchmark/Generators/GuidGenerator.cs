using Bogus;
using DatabaseBenchmark.Generators.Interfaces;

namespace DatabaseBenchmark.Generators
{
    public class GuidGenerator : IGenerator
    {
        private readonly Faker _faker;

        public GuidGenerator(Faker faker)
        {
            _faker = faker;
        }

        public object Generate() => _faker.Random.Guid();
    }
}
