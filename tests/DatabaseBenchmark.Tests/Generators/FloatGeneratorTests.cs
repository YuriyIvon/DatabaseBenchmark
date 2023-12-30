using Bogus;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Generators;
using DatabaseBenchmark.Generators.Options;
using Xunit;

namespace DatabaseBenchmark.Tests.Generators
{
    public class FloatGeneratorTests
    {
        private readonly Faker _faker = new();
        private readonly FloatGeneratorOptions _options = new()
        {
            MinValue = 11,
            MaxValue = 101
        };

        [Fact]
        public void GenerateValueNoDelta()
        {
            var generator = new FloatGenerator(_faker, _options);

            var value = generator.Generate();

            Assert.IsType<double>(value);

            var doubleValue = (double)value;
            Assert.True(doubleValue >= _options.MinValue);
            Assert.True(doubleValue <= _options.MaxValue);
        }

        [Fact]
        public void GenerateValueWithDelta()
        {
            _options.Delta = 2;

            var generator = new FloatGenerator(_faker, _options);

            var value = generator.Generate();

            Assert.IsType<double>(value);

            var doubleValue = (double)value;
            Assert.True(doubleValue >= _options.MinValue);
            Assert.True(doubleValue <= _options.MaxValue);
            Assert.Equal(1, doubleValue % 2);
        }

        [Fact]
        public void GenerateIncreasingValueNoDelta()
        {
            _options.Increasing = true;

            var generator = new FloatGenerator(_faker, _options);

            Assert.Throws<InputArgumentException>(generator.Generate);
        }

        [Fact]
        public void GenerateIncreasingValueWithConstantDelta()
        {
            _options.Delta = 1;
            _options.Increasing = true;

            var generator = new FloatGenerator(_faker, _options);

            double lastValue = _options.MinValue - 1;
            for (int i = 0; i < 10; i++)
            {
                var value = generator.Generate();

                var doubleValue = (double)value;
                Assert.True(doubleValue > lastValue);
                Assert.Equal(1, doubleValue - lastValue);

                lastValue = doubleValue;
            }
        }

        [Fact]
        public void GenerateIncreasingValueWithRandomDelta()
        {
            _options.Delta = 1;
            _options.Increasing = true;
            _options.RandomizeDelta = true;

            var generator = new FloatGenerator(_faker, _options);

            double lastValue = 0;
            for (int i = 0; i < 10; i++)
            {
                var value = generator.Generate();

                var doubleValue = (double)value;
                Assert.True(doubleValue > lastValue);

                lastValue = doubleValue;
            }
        }
    }
}
