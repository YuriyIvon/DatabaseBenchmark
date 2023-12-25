using Bogus;
using DatabaseBenchmark.Generators.Interfaces;

namespace DatabaseBenchmark.Generators
{
    public class CollectionGeneratorDecorator : IGenerator
    {
        private readonly Faker _faker;
        private readonly IGenerator _randomGenerator;
        private readonly int _minCollectionLength;
        private readonly int _maxCollectionLength;

        public CollectionGeneratorDecorator(Faker faker,
            IGenerator randomGenerator,
            int minCollectionLength,
            int maxCollectionLength)
        {
            _faker = faker;
            _randomGenerator = randomGenerator;
            _minCollectionLength = minCollectionLength;
            _maxCollectionLength = maxCollectionLength;
        }

        public object Generate()
        {
            var length = _faker.Random.Int(_minCollectionLength, _maxCollectionLength);

            return Enumerable.Range(1, length)
                .Select(_ => _randomGenerator.Generate())
                .ToArray();
        }
    }
}
