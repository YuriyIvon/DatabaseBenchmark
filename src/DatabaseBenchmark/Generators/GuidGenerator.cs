using Bogus;
using DatabaseBenchmark.Generators.Interfaces;

namespace DatabaseBenchmark.Generators
{
    public class GuidGenerator : IGenerator
    {
        private readonly Faker _faker;

        public object Current { get; private set; }

        public GuidGenerator(Faker faker)
        {
            _faker = faker;
        }

        public bool Next()
        {
            Current = _faker.Random.Guid();

            return true;
        }
    }
}
