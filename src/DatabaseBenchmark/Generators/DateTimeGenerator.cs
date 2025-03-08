using Bogus;
using Bogus.DataSets;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;

namespace DatabaseBenchmark.Generators
{
    public class DateTimeGenerator : IGenerator
    {
        private readonly Randomizer _randomizer = new();
        private readonly Date _dateFaker = new();
        private readonly DateTimeGeneratorOptions _options;

        private DateTime? _lastValue;

        public object Current { get; private set; }

        public bool IsBounded => _options.Direction != Direction.None;

        public DateTimeGenerator(DateTimeGeneratorOptions options)
        {
            _options = options;
        }

        public bool Next()
        {
            var minValue = DateTime.SpecifyKind(_options.MinValue, _options.DateTimeKind);
            var maxValue = DateTime.SpecifyKind(_options.MaxValue, _options.DateTimeKind);

            if (_options.Direction != Direction.None)
            {
                if (_options.Delta.TotalMilliseconds == 0)
                {
                    throw new InputArgumentException("Delta must be set if the direction is not \"None\"");
                }

                if (_lastValue == null)
                {
                    _lastValue = _options.Direction == Direction.Ascending
                        ? minValue
                        : maxValue;
                }
                else
                {
                    var delta = _options.Delta;

                    if (_options.RandomizeDelta)
                    {
                        var milliseconds = _options.Delta.TotalMilliseconds;
                        var randomMilliseconds = _randomizer.Long(1, (long)milliseconds);
                        delta = TimeSpan.FromMilliseconds(randomMilliseconds);
                    }

                    var isAscending = _options.Direction == Direction.Ascending;
                    var value = _lastValue + (isAscending ? delta : -delta);

                    if ((isAscending && value > maxValue) ||
                        (!isAscending && value < minValue))
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
                var rangeMilliseconds = (maxValue - minValue).TotalMilliseconds;
                var totalSegments = (long)(rangeMilliseconds / deltaMilliseconds);
                var randomSegment = _randomizer.Long(0, totalSegments);

                Current = minValue.AddMilliseconds(deltaMilliseconds * randomSegment);

                return true;
            }
            else
            {
                Current = _dateFaker.Between(minValue, maxValue);

                return true;
            }
        }
    }
}
