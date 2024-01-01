using Bogus;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;

namespace DatabaseBenchmark.Generators
{
    public class CollectionGenerator : IGenerator
    {
        private readonly Faker _faker;
        private readonly CollectionGeneratorOptions _options;
        private readonly IGenerator _sourceGenerator;
        private readonly ICollectionGenerator _sourceCollectionGenerator;

        public CollectionGenerator(Faker faker,
            IGenerator sourceGenerator,
            CollectionGeneratorOptions options)
        {
            _faker = faker;
            _options = options;
            _sourceGenerator = sourceGenerator;
            _sourceCollectionGenerator = sourceGenerator as ICollectionGenerator;
        }

        public object Generate()
        {
            var length = _faker.Random.Int(_options.MinLength, _options.MaxLength);

            return _sourceCollectionGenerator != null 
                ? _sourceCollectionGenerator.GenerateCollection(length)
                : Enumerable.Range(1, length)
                    .Select(_ => _sourceGenerator.Generate())
                    .ToArray();
        }
    }
}
