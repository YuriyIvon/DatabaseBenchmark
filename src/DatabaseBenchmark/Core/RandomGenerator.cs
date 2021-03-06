using DatabaseBenchmark.Core.Interfaces;

namespace DatabaseBenchmark.Core
{
    public class RandomGenerator : IRandomGenerator
    {
        private readonly Random _random = new();

        public bool GetRandomBoolean() => _random.NextDouble() >= 0.5;

        public int GetRandomInteger(int minValue, int maxValue) =>
            _random.Next(minValue, maxValue + 1);

        public double GetRandomDouble(double minValue, double maxValue) =>
            minValue + (_random.NextDouble() * (maxValue - minValue));

        public DateTime GetRandomDateTime(DateTime minValue, DateTime maxValue, TimeSpan step)
        {
            double range = (maxValue - minValue).TotalSeconds;
            int steps = (int)(range / step.TotalSeconds);

            return minValue.AddSeconds(_random.Next(steps + 1) * step.TotalSeconds);
        }

        public string GetRandomString(int minLength, int maxLength, string allowedCharacters)
        {
            var length = _random.Next(minLength, maxLength + 1);

            return new string(Enumerable.Range(1, length)
                .Select(_ => allowedCharacters[_random.Next(allowedCharacters.Length)])
                .ToArray());
        }
    }
}
