using Bogus;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;

namespace DatabaseBenchmark.Generators
{
    public class FloatGenerator : IGenerator
    {
        private readonly Faker _faker;
        private readonly FloatGeneratorOptions _options;

        private double? _lastValue;

        public object Current { get; private set; }

        public FloatGenerator(Faker faker, FloatGeneratorOptions options)
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
                        delta = _faker.Random.Double(0, delta); //assuming the result will never be equal to zero
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
                var totalSegments = (int)((_options.MaxValue - _options.MinValue) / _options.Delta);
                var randomSegment = _faker.Random.Int(0, totalSegments);

                Current = _options.MinValue + (_options.Delta * randomSegment);

                return true;
            }
            else
            {
                Current = _faker.Random.Double(_options.MinValue, _options.MaxValue);

                return true;
            }
        }
    }
}
