using Bogus;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Generators;
using DatabaseBenchmark.Generators.Options;
using System.Linq;
using Xunit;

namespace DatabaseBenchmark.Tests.Generators
{
    public class ListItemGeneratorTests
    {
        private readonly Faker _faker = new();

        private readonly object[] _items =
        [
            "item1",
            "item2",
            "item3",
            "item4",
            "item5"

        ];

        private readonly WeightedListItem[] _weightedItems =
        [
            new WeightedListItem { Value = "weightedItem1", Weight = 0.05f },
            new WeightedListItem { Value = "weightedItem2", Weight = 0.1f },
            new WeightedListItem { Value = "weightedItem3", Weight = 0.15f },
            new WeightedListItem { Value = "weightedItem4", Weight = 0.2f },
            new WeightedListItem { Value = "weightedItem5", Weight = 0.25f }
        ];

        [Fact]
        public void GenerateFromItems()
        {
            var generator = new ListItemGenerator(
                _faker,
                new ListItemGeneratorOptions { Items = _items });

            generator.Next();
            var item = generator.Current;

            Assert.Contains(item, _items);
        }

        [Fact]
        public void GenerateFromWeightedItems()
        {
            var generator = new ListItemGenerator(
                _faker,
                new ListItemGeneratorOptions { WeightedItems = _weightedItems });

            generator.Next();
            var item = generator.Current;

            var exists = _weightedItems.Any(i => i.Value == item);
            Assert.True(exists);
        }

        [Fact]
        public void GenerateFromItemsAndWeightedItems()
        {
            var generator = new ListItemGenerator(
                _faker,
                new ListItemGeneratorOptions
                {
                    Items = _items,
                    WeightedItems = _weightedItems
                });

            generator.Next();
            var item = generator.Current;

            Assert.True(_items.Contains(item) || _weightedItems.Any(i => i.Value == item));
        }

        [Fact]
        public void GenerateNoItemsError()
        {
            var generator = new ListItemGenerator(_faker, new ListItemGeneratorOptions());

            Assert.Throws<InputArgumentException>(() => generator.Next());
        }

        [Fact]
        public void GenerateWeightedItemsError()
        {
            _weightedItems[0].Weight = 1;

            var generator = new ListItemGenerator(
                _faker,
                new ListItemGeneratorOptions
                {
                    WeightedItems = _weightedItems
                });

            Assert.Throws<InputArgumentException>(() => generator.Next());
        }

        [Fact]
        public void GenerateCollectionFromItems()
        {
            var generator = new ListItemGenerator(
                _faker,
                new ListItemGeneratorOptions { Items = _items });

            generator.NextCollection(3);
            var collection = generator.CurrentCollection;

            var hasDuplicates = collection.GroupBy(v => v).Any(g => g.Count() > 1);
            Assert.False(hasDuplicates);
            Assert.Equal(3, collection.Count());
            Assert.True(collection.All(i => _items.Contains(i)));
        }
    }
}
