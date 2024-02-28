using DatabaseBenchmark.Generators;
using System;
using Xunit;

namespace DatabaseBenchmark.Tests.Generators
{
    public class GuidGeneratorTests
    {
        [Fact]
        public void GenerateValue()
        {
            var generator = new GuidGenerator();

            generator.Next();

            Assert.IsType<Guid>(generator.Current);
        }
    }
}
