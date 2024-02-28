using DatabaseBenchmark.Generators;
using DatabaseBenchmark.Generators.Options;
using Xunit;

namespace DatabaseBenchmark.Tests.Generators
{
    public class ConstantGeneratorTests
    {
        [Fact]
        public void GenerateStringConstant()
        {
            var options = new ConstantGeneratorOptions { Value = "String" };
            var generator = new ConstantGenerator(options);

            generator.Next();
            var value = generator.Current;

            Assert.IsType<string>(value);
            Assert.Equal(options.Value, value);
        }

        [Fact]
        public void GenerateBooleanConstant()
        {
            var options = new ConstantGeneratorOptions { Value = true };
            var generator = new ConstantGenerator(options);

            generator.Next();
            var value = generator.Current;

            Assert.IsType<bool>(value);
            Assert.Equal(options.Value, value);
        }
    }
}
