using Bogus;
using DatabaseBenchmark.Generators;
using System;
using Xunit;

namespace DatabaseBenchmark.Tests.Generators
{
    public class GuidGeneratorTests
    {
        private readonly Faker _faker = new();

        [Fact]
        public void GenerateValue()
        {
            var generator = new GuidGenerator(_faker);

            var value = generator.Generate();

            Assert.IsType<Guid>(value);
        }
    }
}
