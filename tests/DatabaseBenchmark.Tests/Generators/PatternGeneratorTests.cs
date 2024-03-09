using DatabaseBenchmark.Generators;
using DatabaseBenchmark.Generators.Options;
using Xunit;

namespace DatabaseBenchmark.Tests.Generators
{
    public class PatternGeneratorTests
    {
        [Fact]
        public void GenerateValueSimplePattern()
        {
            var options = new PatternGeneratorOptions { Pattern = "###-****-?????" };
            var generator = new PatternGenerator(options);

            generator.Next();
            var value = generator.Current;

            Assert.IsType<string>(value);

            var stringValue = (string)value;
            Assert.Matches("^[0-9]{3}-[A-Z0-9]{4}-[A-Z]{5}$", stringValue);
        }

        [Fact]
        public void GenerateValueRegexPattern()
        {
            var options = new PatternGeneratorOptions
            { 
                Pattern = @"^[A-F]{4}[+-]\d{2,4}$",
                Kind = PatternGeneratorOptions.GeneratorKind.Regex
            };

            var generator = new PatternGenerator(options);

            generator.Next();
            var value = generator.Current;

            Assert.IsType<string>(value);

            var stringValue = (string)value;
            Assert.Matches(options.Pattern, stringValue);
        }
    }
}
