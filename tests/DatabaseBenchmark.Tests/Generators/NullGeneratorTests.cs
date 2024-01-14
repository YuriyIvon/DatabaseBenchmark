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

            _baseGenerator.Next();
            _baseGenerator.Current.Returns(1);
        }

        public void GenerateNull()
        {
            var generator = new NullGenerator(_faker, new NullGeneratorOptions { Weight = 1 }, _baseGenerator);
            generator.Next();

            Assert.Null(generator.Current);
        }

        public void GenerateNotNull()
        {
            var generator = new NullGenerator(_faker, new NullGeneratorOptions { Weight = 0 }, _baseGenerator);
            generator.Next();

            Assert.Equal(1, generator.Current);
        }
    }
}
