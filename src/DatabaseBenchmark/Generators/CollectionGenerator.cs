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

        public object Current { get; private set; }

        public CollectionGenerator(
            Faker faker,
            IGenerator sourceGenerator,
            CollectionGeneratorOptions options)
        {
            _faker = faker;
            _options = options;
            _sourceGenerator = sourceGenerator;
            _sourceCollectionGenerator = sourceGenerator as ICollectionGenerator;
        }

        public bool Next()
        {
            var length = _faker.Random.Int(_options.MinLength, _options.MaxLength);

            if (_sourceCollectionGenerator != null)
            {
                if (!_sourceCollectionGenerator.NextCollection(length))
                {
                    return false;
                }

                Current = _sourceCollectionGenerator.CurrentCollection;

                return true;
            }
            else
            {
                var collection = new object[length];

                for (int i = 0; i < length; i++)
                {
                    if (!_sourceGenerator.Next())
                    {
                        return false;
                    }

                    collection[i] = _sourceGenerator.Current;
                }

                Current = collection;

                return true;
            }
        }
    }
}
