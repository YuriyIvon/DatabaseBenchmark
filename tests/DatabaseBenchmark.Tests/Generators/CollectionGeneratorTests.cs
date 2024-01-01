using Bogus;
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

        private readonly Faker _faker = new();

        [Fact]
        public void GenerateCollectionWithValueGenerator()
        {
            var collectionGenerator = new DatabaseBenchmark.Generators.CollectionGenerator(
                _faker, 
                new ValueGenerator(),
                new CollectionGeneratorOptions { MinLength = MinLength, MaxLength = MaxLength });

            var collection = (IEnumerable<object>)collectionGenerator.Generate();

            Assert.True(collection.Count() >= MinLength);
            Assert.True(collection.Count() <= MaxLength);
            Assert.True(collection.All(i => (string)i == SingleValue));
        }

        [Fact]
        public void GenerateCollectionWithCollectionGenerator()
        {
            var collectionGenerator = new DatabaseBenchmark.Generators.CollectionGenerator(
                _faker,
                new CollectionGenerator(),
                new CollectionGeneratorOptions { MinLength = MinLength, MaxLength = MaxLength });

            var collection = (IEnumerable<object>)collectionGenerator.Generate();

            Assert.True(collection.Count() >= MinLength);
            Assert.True(collection.Count() <= MaxLength);
            Assert.True(collection.All(i => (string)i == CollectionValue));
        }

        private class ValueGenerator : IGenerator
        {
            public object Generate() => SingleValue;
        }

        private class CollectionGenerator : IGenerator, ICollectionGenerator
        {
            public object Generate() => SingleValue;

            public IEnumerable<object> GenerateCollection(int length) =>
                Enumerable.Repeat(CollectionValue, length);
        }
    }
}
