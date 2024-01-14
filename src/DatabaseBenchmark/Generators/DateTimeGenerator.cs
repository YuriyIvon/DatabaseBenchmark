using Bogus;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;

namespace DatabaseBenchmark.Generators
{
    public class DateTimeGenerator : IGenerator
    {
        private readonly Faker _faker;
        private readonly DateTimeGeneratorOptions _options;

        private DateTime? _lastValue;

        public object Current { get; private set; }

        public DateTimeGenerator(Faker faker, DateTimeGeneratorOptions options)
        {
            _faker = faker;
            _options = options;
        }

        public bool Next()
        {
            if (_options.Increasing)
            {
                if (_options.Delta.TotalMilliseconds == 0)
                {
                    throw new InputArgumentException("Delta must be set for the \"Increasing\" generator mode");
                }

                if (_lastValue == null)
                {
                    _lastValue = _options.MinValue;
                }
                else
                {
                    var delta = _options.Delta;

                    if (_options.RandomizeDelta)
                    {
                        var milliseconds = _options.Delta.TotalMilliseconds;
                        var randomMilliseconds = _faker.Random.Long(1, (long)milliseconds);
                        delta = TimeSpan.FromMilliseconds(randomMilliseconds);
                    }

                    var value = _lastValue + delta;

                    if (value > _options.MaxValue)
                    {
                        return false;
                    }

                    _lastValue = value;
                }

                Current = _lastValue;

                return true;
            }
            else if (_options.Delta.TotalMilliseconds != 0)
            {
                var deltaMilliseconds = _options.Delta.TotalMilliseconds;
                var rangeMilliseconds = (_options.MaxValue - _options.MinValue).TotalMilliseconds;
                var totalSegments = (long)(rangeMilliseconds / deltaMilliseconds);
                var randomSegment = _faker.Random.Long(0, totalSegments);

                Current = _options.MinValue.AddMilliseconds(deltaMilliseconds * randomSegment);

                return true;
            }
            else
            {
                Current = _faker.Date.Between(_options.MinValue, _options.MaxValue);

                return true;
            }
        }
    }
}
