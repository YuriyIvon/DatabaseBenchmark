using Bogus;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;

namespace DatabaseBenchmark.Generators
{
    public class CollectionGenerator : IGenerator
    {
        private readonly Randomizer _randomizer = new();
        private readonly CollectionGeneratorOptions _options;
        private readonly IGenerator _sourceGenerator;
        private readonly ICollectionGenerator _sourceCollectionGenerator;

        public object Current { get; private set; }

        public CollectionGenerator(
            IGenerator sourceGenerator,
            CollectionGeneratorOptions options)
        {
            _options = options;
            _sourceGenerator = sourceGenerator;
            _sourceCollectionGenerator = sourceGenerator as ICollectionGenerator;
        }

        public bool Next()
        {
            var length = _randomizer.Int(_options.MinLength, _options.MaxLength);

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
