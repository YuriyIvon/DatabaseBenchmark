using DatabaseBenchmark.Generators;
using DatabaseBenchmark.Generators.Options;
using System.Collections.Generic;
using Xunit;

namespace DatabaseBenchmark.Tests.Generators
{
    public class ListIteratorGeneratorTests
    {
        private readonly object[] _items =
        [
            "item1",
            "item2",
            "item3",
            "item4",
            "item5"
        ];

        [Fact]
        public void GenerateValue()
        {
            var generator = new ListIteratorGenerator(
                new ListIteratorGeneratorOptions { Items = _items });
            var items = new List<object>();

            for (int i = 0; i < _items.Length + 10 && generator.Next(); i++)
            {
                items.Add(generator.Current);
            }

            Assert.Equal(_items, items);
            Assert.False(generator.Next());
        }
    }
}
