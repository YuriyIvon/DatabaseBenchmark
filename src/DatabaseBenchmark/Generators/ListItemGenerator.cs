using Bogus;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;

namespace DatabaseBenchmark.Generators
{
    public class ListItemGenerator : IGenerator, ICollectionGenerator
    {
        private readonly Randomizer _randomizer = new();
        private readonly ListItemGeneratorOptions _options;

        private float _totalWeight;
        private object[] _items;
        private object[] _weightedItems;
        private float[] _weights;

        public object Current { get; private set; }

        public IEnumerable<object> CurrentCollection { get; private set; }

        public ListItemGenerator(ListItemGeneratorOptions options)
        {
            _options = options;
        }

        public bool Next()
        {
            if (_items == null)
            {
                Initialize();
            }

            Current = _weightedItems != null
                ? _randomizer.WeightedRandom(_weightedItems, _weights)
                : _randomizer.ArrayElement(_items);

            return true;
        }

        public bool NextCollection(int length)
        {
            if (_items == null)
            {
                Initialize();
            }

            if (_options.WeightedItems?.Any() == true)
            {
                throw new InputArgumentException("Generating a collection based on item weights is not supported");
            }

            CurrentCollection = _randomizer.ArrayElements(_items, length);

            return true;
        }

        private void Initialize()
        {
            if (_options.Items?.Any() != true && _options.WeightedItems?.Any() != true)
            {
                throw new InputArgumentException("Neither items nor weighted items are specified for the list item generator");
            }

            if (_options.WeightedItems?.Any() == true)
            {
                var weightedItems = _options.WeightedItems.Select(i => i.Value).ToList();
                var weights = _options.WeightedItems.Select(i => i.Weight).ToList();
                _totalWeight = weights.Sum();

                if (_totalWeight > 1)
                {
                    throw new InputArgumentException("The total weight of the weighted items can't exceed 1");
                }

                if (_options.Items?.Any() == true)
                {
                    _items = _options.Items.Except(weightedItems).ToArray();
                }

                if (_totalWeight < 1)
                {
                    if (_items?.Any() != true)
                    {
                        throw new InputArgumentException("The total weight is lower than 1, but there are no non-weighted items provided");
                    }

                    var remainingItemWeight = (1 - _totalWeight) / _items.Length;
                    weightedItems.AddRange(_items);
                    weights.AddRange(Enumerable.Repeat(remainingItemWeight, _items.Length));
                }

                _weightedItems = weightedItems.ToArray();
                _weights = weights.ToArray();
            }
            else
            {
                _items = _options.Items;
            }
        }
    }
}
