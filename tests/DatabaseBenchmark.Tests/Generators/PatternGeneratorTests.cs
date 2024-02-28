using Bogus;
using DatabaseBenchmark.Generators;
using DatabaseBenchmark.Generators.Options;
using Xunit;

namespace DatabaseBenchmark.Tests.Generators
{
    public class PatternGeneratorTests
    {
        private readonly Faker _faker = new();

        [Fact]
        public void GenerateValue()
        {
            var options = new PatternGeneratorOptions { Pattern = "###-****-?????" };
            var generator = new PatternGenerator(_faker, options);

            generator.Next();
            var value = generator.Current;

            Assert.IsType<string>(value);

            var stringValue = (string)value;
            Assert.Matches("^[0-9]{3}-[A-Z0-9]{4}-[A-Z]{5}$", stringValue);
        }
    }
}
