using Bogus;
using DatabaseBenchmark.Generators;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using NSubstitute;
using Xunit;

namespace DatabaseBenchmark.Tests.Generators
{
    public class NullGeneratorTests
    {
        private readonly Faker _faker = new();
        private readonly IGenerator _baseGenerator;

        public NullGeneratorTests()
        {
            _baseGenerator = Substitute.For<IGenerator>();
            _baseGenerator.Generate().Returns(1);
        }

        public void GenerateNull()
        {
            var generator = new NullGenerator(_faker, new NullGeneratorOptions { NullProbability = 1 }, _baseGenerator);
            var value = generator.Generate();

            Assert.Null(value);
        }

        public void GenerateNotNull()
        {
            var generator = new NullGenerator(_faker, new NullGeneratorOptions { NullProbability = 0 }, _baseGenerator);
            var value = generator.Generate();

            Assert.Equal(1, value);
        }
    }
}
