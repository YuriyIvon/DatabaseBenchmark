using Bogus;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;

namespace DatabaseBenchmark.Generators
{
    public class ListItemGenerator : IGenerator, ICollectionGenerator
    {
        private readonly Faker _faker;
        private readonly ListItemGeneratorOptions _options;

        private float _totalWeight;
        private object[] _items;
        private object[] _weightedItems;
        private float[] _weights;

        public ListItemGenerator(Faker faker, ListItemGeneratorOptions options)
        {
            _faker = faker;
            _options = options;
        }

        public object Generate()
        {
            if (_items == null)
            {
                Initialize();
            }

            var useWeighted = _items == null || !_items.Any() || _faker.Random.Bool(_totalWeight);

            return useWeighted
                ? _faker.Random.WeightedRandom(_weightedItems, _weights)
                : _faker.Random.ArrayElement(_items);
        }

        public IEnumerable<object> GenerateCollection(int length)
        {
            if (_items == null)
            {
                Initialize();
            }

            if (_options.WeightedItems?.Any() == true)
            {
                throw new InputArgumentException("Generating a collection based on item weights is not supported");
            }

            return  _faker.Random.ArrayElements(_items, length);
        }

        private void Initialize()
        {
            if (_options.Items?.Any() != true && _options.WeightedItems?.Any() != true)
            {
                throw new InputArgumentException("Neither items nor weighted items are specified for the list item generator");
            }

            if (_options.WeightedItems?.Any() == true)
            {
                _weightedItems = _options.WeightedItems.Select(i => i.Value).ToArray();
                _weights = _options.WeightedItems.Select(i => i.Weight).ToArray();
                _totalWeight = _weights.Sum();

                if (_options.Items?.Any() == true)
                {
                    _items = _options.Items.Except(_weightedItems).ToArray();
                }
            }
            else
            {
                _items = _options.Items;
            }
        }
    }
}
