using Bogus;
using DatabaseBenchmark.Generators.Interfaces;

namespace DatabaseBenchmark.Generators
{
    public class CollectionGeneratorDecorator : IGenerator
    {
        private readonly Faker _faker;
        private readonly IGenerator _generator;
        private readonly ICollectionGenerator _collectionGenerator;
        private readonly int _minLength;
        private readonly int _maxLength;

        public CollectionGeneratorDecorator(Faker faker,
            IGenerator generator,
            int minLength,
            int maxLength)
        {
            _faker = faker;
            _generator = generator;
            _collectionGenerator = generator as ICollectionGenerator;
            _minLength = minLength;
            _maxLength = maxLength;
        }

        public object Generate()
        {
            var length = _faker.Random.Int(_minLength, _maxLength);

            return _collectionGenerator != null 
                ? _collectionGenerator.GenerateCollection(length)
                : Enumerable.Range(1, length)
                    .Select(_ => _generator.Generate())
                    .ToArray();
        }
    }
}
