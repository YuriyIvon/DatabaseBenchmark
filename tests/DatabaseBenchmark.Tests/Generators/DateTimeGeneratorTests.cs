using Bogus;
using DatabaseBenchmark.Generators;
using DatabaseBenchmark.Generators.Options;
using System;
using Xunit;

namespace DatabaseBenchmark.Tests.Generators
{
    public class DateTimeGeneratorTests
    {
        private readonly Faker _faker = new();
        private readonly DateTimeGeneratorOptions _options = new()
        {
            MinValue = DateTime.Now,
            MaxValue = DateTime.Now.AddYears(1)
        };

        [Fact]
        public void GenerateValue()
        {
            var generator = new DateTimeGenerator(_faker, _options);

            var value = generator.Generate();

            Assert.IsType<DateTime>(value);

            var dtValue = (DateTime)value;
            Assert.True(dtValue >= _options.MinValue);
            Assert.True(dtValue <= _options.MaxValue);
        }

        [Fact]
        public void GenerateValueConstantDelta()
        {
            DateTime maxValue = DateTime.Now;
            DateTime minValue = maxValue.AddYears(-10);

            DateTimeGeneratorOptions BuildGeneratorOptions(TimeSpan delta) =>
                new()
                {
                    MinValue = minValue,
                    MaxValue = maxValue,
                    Delta = delta
                };

            var dailyGenerator = new DateTimeGenerator(_faker, BuildGeneratorOptions(TimeSpan.FromDays(1)));
            var hourlyGenerator = new DateTimeGenerator(_faker, BuildGeneratorOptions(TimeSpan.FromHours(1)));
            var minutelyGenerator = new DateTimeGenerator(_faker, BuildGeneratorOptions(TimeSpan.FromMinutes(1)));

            var dailyValue = (DateTime)dailyGenerator.Generate();
            var hourlyValue = (DateTime)hourlyGenerator.Generate();
            var minutelyValue = (DateTime)minutelyGenerator.Generate();

            Assert.True(dailyValue < maxValue);
            Assert.True(dailyValue >= minValue);
            Assert.True(hourlyValue < maxValue);
            Assert.True(hourlyValue >= minValue);
            Assert.True(minutelyValue < maxValue);
            Assert.True(minutelyValue >= minValue);

            Assert.Equal(maxValue.Hour, dailyValue.Hour);
            Assert.Equal(maxValue.Minute, dailyValue.Minute);
            Assert.Equal(maxValue.Second, dailyValue.Second);

            Assert.Equal(maxValue.Minute, hourlyValue.Minute);
            Assert.Equal(maxValue.Second, hourlyValue.Second);

            Assert.Equal(maxValue.Second, minutelyValue.Second);
        }

        [Fact]
        public void GenerateIncreasingValueConstantDelta()
        {
            _options.Delta = TimeSpan.FromDays(1);
            _options.Increasing = true;

            var generator = new DateTimeGenerator(_faker, _options);

            DateTime lastValue = _options.MinValue.AddDays(-1);
            for (int i = 0; i < 10; i++)
            {
                var value = generator.Generate();

                var dtValue = (DateTime)value;
                Assert.True(dtValue > lastValue);
                Assert.Equal(TimeSpan.FromDays(1), dtValue - lastValue);

                lastValue = dtValue;
            }
        }

        [Fact]
        public void GenerateIncreasingValueRandomDelta()
        {
            _options.Delta = TimeSpan.FromDays(1);
            _options.Increasing = true;
            _options.RandomizeDelta = true;

            var generator = new DateTimeGenerator(_faker, _options);

            DateTime lastValue = DateTime.MinValue;
            for (int i = 0; i < 10; i++)
            {
                var value = generator.Generate();

                var dtValue = (DateTime)value;
                Assert.True(dtValue > lastValue);

                lastValue = dtValue;
            }
        }
    }
}
