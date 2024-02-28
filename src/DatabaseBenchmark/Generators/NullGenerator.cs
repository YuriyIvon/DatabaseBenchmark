using Bogus;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;

namespace DatabaseBenchmark.Generators
{
    public class NullGenerator : IGenerator, IDisposable
    {
        private readonly Randomizer _randomizer = new();
        private readonly NullGeneratorOptions _options;
        private readonly IGenerator _sourceGenerator;

        public object Current { get; private set; }

        public NullGenerator(NullGeneratorOptions options, IGenerator sourceGenerator)
        {
            _options = options;
            _sourceGenerator = sourceGenerator;
        }

        public bool Next()
        {
            if (_randomizer.Bool(_options.Weight))
            {
                Current = null;
            }
            else
            {
                if (!_sourceGenerator.Next())
                {
                    return false;
                }

                Current = _sourceGenerator.Current;
            }

            return true;
        }

        public void Dispose()
        {
            if (_sourceGenerator is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
