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

        public IntegerGenerator(Faker faker, IntegerGeneratorOptions options)
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
                        delta = _faker.Random.Int(1, delta);
                    }

                    _lastValue += delta;
                }

                return _lastValue;
            }
            else if (_options.Delta != 0)
            {
                var totalSegments = (_options.MaxValue - _options.MinValue) / _options.Delta;
                var randomSegment = _faker.Random.Int(0, totalSegments);

                return _options.MinValue + (_options.Delta * randomSegment);
            }
            else
            {
                return _faker.Random.Int(_options.MinValue, _options.MaxValue);
            }
        }
    }
}
