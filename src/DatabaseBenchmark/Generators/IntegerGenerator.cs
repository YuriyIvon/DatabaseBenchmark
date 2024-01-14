using Bogus;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;

namespace DatabaseBenchmark.Generators
{
    public class IntegerGenerator : IGenerator
    {
        private readonly Faker _faker;
        private readonly IntegerGeneratorOptions _options;

        private int? _lastValue;

        public object Current { get; private set; }

        public IntegerGenerator(Faker faker, IntegerGeneratorOptions options)
        {
            _faker = faker;
            _options = options;
        }

        public bool Next()
        {
            if (_options.Increasing)
            {
                if (_options.Delta == 0)
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
                        delta = _faker.Random.Int(1, delta);
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
            else if (_options.Delta != 0)
            {
                var totalSegments = (_options.MaxValue - _options.MinValue) / _options.Delta;
                var randomSegment = _faker.Random.Int(0, totalSegments);

                Current = _options.MinValue + (_options.Delta * randomSegment);

                return true;
            }
            else
            {
                Current = _faker.Random.Int(_options.MinValue, _options.MaxValue);

                return true;
            }
        }
    }
}
