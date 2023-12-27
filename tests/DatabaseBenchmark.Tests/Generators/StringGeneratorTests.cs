using Bogus;
using DatabaseBenchmark.Generators;
using DatabaseBenchmark.Generators.Options;
using System.Linq;
using Xunit;

namespace DatabaseBenchmark.Tests.Generators
{
    public class StringGeneratorTests
    {
        private readonly Faker _faker = new();
        private readonly StringGeneratorOptions _options = new()
        {
            MinLength = 11,
            MaxLength = 23,
            AllowedCharacters = "ABCDEF12345678"
        };

        [Fact]
        public void GenerateValue()
        {
            var generator = new StringGenerator(_faker, _options);

            var value = generator.Generate();

            Assert.IsType<string>(value);

            var stringValue = (string)value;
            Assert.True(stringValue.Length >= _options.MinLength);
            Assert.True(stringValue.Length <= _options.MaxLength);
            Assert.True(stringValue.All(_options.AllowedCharacters.Contains));
        }

        [Fact]
        public void GenerateFixedLengthValue()
        {
            _options.MaxLength = _options.MinLength;
            var generator = new StringGenerator(_faker, _options);

            var value = generator.Generate();

            Assert.IsType<string>(value);

            var stringValue = (string)value;
            Assert.Equal(_options.MinLength, stringValue.Length);
            Assert.True(stringValue.All(_options.AllowedCharacters.Contains));
        }
    }
}
