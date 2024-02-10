using Bogus;
using DatabaseBenchmark.Generators;
using DatabaseBenchmark.Generators.Options;
using System.Collections.Generic;
using Xunit;

namespace DatabaseBenchmark.Tests.Generators
{
    public class UniqueGeneratorTests
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

        [Fact]
        public void GenerateValueFromList()
        {
            var generator = new UniqueGenerator(new UniqueGeneratorOptions(),
                new ListItemGenerator(_faker, new ListItemGeneratorOptions 
                {
                    Items = _items
                }));

            var resultSet = new HashSet<object>();

            for (var i = 0; i < _items.Length + 10 && generator.Next(); i++)
            {
                resultSet.Add(generator.Current);
            }

            Assert.True(resultSet.SetEquals(_items));
        }
    }
}
