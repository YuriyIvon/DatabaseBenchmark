using Bogus;
using DatabaseBenchmark.Databases.Common.Interfaces;

namespace DatabaseBenchmark.Databases.Common
{
    public class RandomPrimitives : IRandomPrimitives
    {
        private readonly Randomizer _randomizer = new();

        public bool GetRandomBoolean() => _randomizer.Bool();
    }
}
