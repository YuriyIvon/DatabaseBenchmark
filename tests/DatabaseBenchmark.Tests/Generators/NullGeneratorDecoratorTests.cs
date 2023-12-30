using Bogus;
using DatabaseBenchmark.Generators;
using DatabaseBenchmark.Generators.Interfaces;
using NSubstitute;
using Xunit;

namespace DatabaseBenchmark.Tests.Generators
{
    public class NullGeneratorDecoratorTests
    {
        private readonly Faker _faker = new();
        private readonly IGenerator _baseGenerator;

        public NullGeneratorDecoratorTests()
        {
            _baseGenerator = Substitute.For<IGenerator>();
            _baseGenerator.Generate().Returns(1);
        }

        public void GenerateNull()
        {
            var generator = new NullGeneratorDecorator(_faker, _baseGenerator, 1);
            var value = generator.Generate();

            Assert.Null(value);
        }

        public void GenerateNotNull()
        {
            var generator = new NullGeneratorDecorator(_faker, _baseGenerator, 0);
            var value = generator.Generate();

            Assert.Equal(1, value);
        }
    }
}
