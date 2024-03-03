using Bogus;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;

namespace DatabaseBenchmark.Generators
{
    public class IntegerGenerator : IGenerator
    {
        private readonly Randomizer _randomizer = new();
        private readonly IntegerGeneratorOptions _options;

        private int? _lastValue;

        public object Current { get; private set; }

        public bool IsBounded => _options.Direction != Direction.None;

        public IntegerGenerator(IntegerGeneratorOptions options)
        {
            _options = options;
        }

        public bool Next()
        {
            if (_options.Direction != Direction.None)
            {
                if (_options.Delta == 0)
                {
                    throw new InputArgumentException("Delta must be set if the direction is not \"None\"");
                }

                if (_lastValue == null)
                {
                    _lastValue = _options.Direction == Direction.Ascending
                        ? _options.MinValue
                        : _options.MaxValue;
                }
                else
                {
                    var delta = _options.Delta;

                    if (_options.RandomizeDelta)
                    {
                        delta = _randomizer.Int(1, delta);
                    }

                    var isAscending = _options.Direction == Direction.Ascending;
                    var value = _lastValue + (isAscending ? delta : -delta);

                    if ((isAscending && value > _options.MaxValue) ||
                        (!isAscending && value < _options.MinValue))
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
                var randomSegment = _randomizer.Int(0, totalSegments);

                Current = _options.MinValue + (_options.Delta * randomSegment);

                return true;
            }
            else
            {
                Current = _randomizer.Int(_options.MinValue, _options.MaxValue);

                return true;
            }
        }
    }
}
