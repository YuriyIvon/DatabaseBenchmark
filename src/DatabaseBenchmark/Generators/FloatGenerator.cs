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

        public FloatGenerator(Faker faker, FloatGeneratorOptions options)
        {
            _faker = faker;
            _options = options;
        }

        public object Generate()
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

                    _lastValue += delta;
                }

                return _lastValue;
            }
            else if (_options.Delta != 0)
            {
                var totalSegments = (int)((_options.MaxValue - _options.MinValue) / _options.Delta);
                var randomSegment = _faker.Random.Int(0, totalSegments);

                return _options.MinValue + (_options.Delta * randomSegment);
            }
            else
            {
                return _faker.Random.Double(_options.MinValue, _options.MaxValue);
            }
        }
    }
}
