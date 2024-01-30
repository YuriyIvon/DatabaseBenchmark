using Bogus;
using DatabaseBenchmark.Databases.Common.Interfaces;

namespace DatabaseBenchmark.Databases.Common
{
    public class RandomPrimitives : IRandomPrimitives
    {
        private readonly Faker _faker = new();

        public bool GetRandomBoolean() => _faker.Random.Bool();
    }
}
