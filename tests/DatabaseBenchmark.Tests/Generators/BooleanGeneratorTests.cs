using DatabaseBenchmark.Generators;
using DatabaseBenchmark.Generators.Options;
using Xunit;

namespace DatabaseBenchmark.Tests.Generators
{
    public class BooleanGeneratorTests
    {
        [Fact]
        public void GenerateValue()
        {
            var generator = new BooleanGenerator(new BooleanGeneratorOptions());

            generator.Next();

            Assert.IsType<bool>(generator.Current);
        }
    }
}
