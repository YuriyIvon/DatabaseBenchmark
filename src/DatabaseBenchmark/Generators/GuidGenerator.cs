using Bogus;
using DatabaseBenchmark.Generators.Interfaces;

namespace DatabaseBenchmark.Generators
{
    public class GuidGenerator : IGenerator
    {
        private readonly Randomizer _randomizer = new();

        public object Current { get; private set; }

        public bool IsBounded => false;

        public bool Next()
        {
            Current = _randomizer.Guid();

            return true;
        }
    }
}
