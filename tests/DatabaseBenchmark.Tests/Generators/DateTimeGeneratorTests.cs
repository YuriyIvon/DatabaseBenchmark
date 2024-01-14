using Bogus;
using DatabaseBenchmark.Common;
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
        public void GenerateValueNoDelta()
        {
            var generator = new DateTimeGenerator(_faker, _options);

            generator.Next();
            var value = generator.Current;

            Assert.IsType<DateTime>(value);

            var dtValue = (DateTime)value;
            Assert.True(dtValue >= _options.MinValue);
            Assert.True(dtValue <= _options.MaxValue);
        }

        [Fact]
        public void GenerateValueWithDelta()
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

            dailyGenerator.Next();
            hourlyGenerator.Next();
            minutelyGenerator.Next();

            var dailyValue = (DateTime)dailyGenerator.Current;
            var hourlyValue = (DateTime)hourlyGenerator.Current;
            var minutelyValue = (DateTime)minutelyGenerator.Current;

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
        public void GenerateIncreasingValueNoDelta()
        {
            _options.Increasing = true;

            var generator = new DateTimeGenerator(_faker, _options);

            Assert.Throws<InputArgumentException>(() => generator.Next());
        }

        [Fact]
        public void GenerateIncreasingValueWithConstantDelta()
        {
            _options.Delta = TimeSpan.FromDays(1);
            _options.Increasing = true;

            var generator = new DateTimeGenerator(_faker, _options);

            DateTime lastValue = _options.MinValue.AddDays(-1);
            for (int i = 0; i < 10; i++)
            {
                generator.Next();

                var dtValue = (DateTime)generator.Current;
                Assert.True(dtValue > lastValue);
                Assert.Equal(TimeSpan.FromDays(1), dtValue - lastValue);

                lastValue = dtValue;
            }
        }

        [Fact]
        public void GenerateIncreasingValueWithRandomDelta()
        {
            _options.Delta = TimeSpan.FromDays(1);
            _options.Increasing = true;
            _options.RandomizeDelta = true;

            var generator = new DateTimeGenerator(_faker, _options);

            DateTime lastValue = DateTime.MinValue;
            for (int i = 0; i < 10; i++)
            {
                generator.Next();

                var dtValue = (DateTime)generator.Current;
                Assert.True(dtValue > lastValue);

                lastValue = dtValue;
            }
        }

        [Fact]
        public void GenerateIncreasingValueWithMaxValue()
        {
            _options.MinValue = DateTime.Now;
            _options.MaxValue = _options.MinValue.AddDays(10);
            _options.Delta = TimeSpan.FromDays(1);
            _options.Increasing = true;

            var generator = new DateTimeGenerator(_faker, _options);

            DateTime i = _options.MinValue;
            for (; i < _options.MaxValue.AddDays(10) && generator.Next(); i = i.AddDays(1))
            {
            }

            Assert.Equal(_options.MaxValue, generator.Current);
            Assert.False(generator.Next());
            Assert.Equal(_options.MaxValue, generator.Current);
        }
    }
}
