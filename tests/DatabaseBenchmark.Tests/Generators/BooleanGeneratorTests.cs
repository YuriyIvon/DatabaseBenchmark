using Bogus;
using DatabaseBenchmark.Generators;
using DatabaseBenchmark.Generators.Options;
using Xunit;

namespace DatabaseBenchmark.Tests.Generators
{
    public class BooleanGeneratorTests
    {
        private readonly Faker _faker = new();

        [Fact]
        public void GenerateValue()
        {
            var generator = new BooleanGenerator(_faker, new BooleanGeneratorOptions());

            generator.Next();

            Assert.IsType<bool>(generator.Current);
        }
    }
}
