using Bogus;
using DatabaseBenchmark.Generators.Interfaces;

namespace DatabaseBenchmark.Generators
{
    public class RandomPrimitives : IRandomPrimitives
    {
        private readonly Faker _faker = new();

        public bool GetRandomBoolean() => _faker.Random.Bool();
    }
}
