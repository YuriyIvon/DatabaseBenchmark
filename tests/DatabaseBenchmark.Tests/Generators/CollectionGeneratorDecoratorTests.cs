using Bogus;
using DatabaseBenchmark.Generators;
using DatabaseBenchmark.Generators.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DatabaseBenchmark.Tests.Generators
{
    public class CollectionGeneratorDecoratorTests
    {
        private const string SingleValue = "value";
        private const string CollectionValue = "collection";
        private const int MinLength = 3;
        private const int MaxLength = 7;

        private readonly Faker _faker = new();

        [Fact]
        public void GenerateCollectionWithValueGenerator()
        {
            var collectionGenerator = new CollectionGeneratorDecorator(_faker, new ValueGenerator(), MinLength, MaxLength);

            var collection = (IEnumerable<object>)collectionGenerator.Generate();

            Assert.True(collection.Count() >= MinLength);
            Assert.True(collection.Count() <= MaxLength);
            Assert.True(collection.All(i => (string)i == SingleValue));
        }

        [Fact]
        public void GenerateCollectionWithCollectionGenerator()
        {
            var collectionGenerator = new CollectionGeneratorDecorator(_faker, new CollectionGenerator(), MinLength, MaxLength);

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
