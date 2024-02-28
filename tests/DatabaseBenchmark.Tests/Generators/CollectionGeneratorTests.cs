using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DatabaseBenchmark.Tests.Generators
{
    public class CollectionGeneratorTests
    {
        private const string SingleValue = "value";
        private const string CollectionValue = "collection";
        private const int MinLength = 3;
        private const int MaxLength = 7;

        [Fact]
        public void GenerateCollectionWithValueGenerator()
        {
            var collectionGenerator = new DatabaseBenchmark.Generators.CollectionGenerator(
                new ValueGenerator(),
                new CollectionGeneratorOptions { MinLength = MinLength, MaxLength = MaxLength });

            collectionGenerator.Next();
            var collection = (IEnumerable<object>)collectionGenerator.Current;

            Assert.True(collection.Count() >= MinLength);
            Assert.True(collection.Count() <= MaxLength);
            Assert.True(collection.All(i => (string)i == SingleValue));
        }

        [Fact]
        public void GenerateCollectionWithCollectionGenerator()
        {
            var collectionGenerator = new DatabaseBenchmark.Generators.CollectionGenerator(
                new CollectionGenerator(),
                new CollectionGeneratorOptions { MinLength = MinLength, MaxLength = MaxLength });

            collectionGenerator.Next();
            var collection = (IEnumerable<object>)collectionGenerator.Current;

            Assert.True(collection.Count() >= MinLength);
            Assert.True(collection.Count() <= MaxLength);
            Assert.True(collection.All(i => (string)i == CollectionValue));
        }

        private class ValueGenerator : IGenerator
        {
            public object Current => SingleValue;

            public bool Next() => true;
        }

        private class CollectionGenerator : IGenerator, ICollectionGenerator
        {
            public object Current => SingleValue;

            public IEnumerable<object> CurrentCollection { get; private set; }

            public bool Next() => true;

            public bool NextCollection(int length)
            {
                CurrentCollection = Enumerable.Repeat(CollectionValue, length);

                return true;
            }
        }
    }
}
