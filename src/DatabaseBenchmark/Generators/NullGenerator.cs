using Bogus;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;

namespace DatabaseBenchmark.Generators
{
    public class NullGenerator : IGenerator, IDisposable
    {
        private readonly Faker _faker;
        private readonly NullGeneratorOptions _options;
        private readonly IGenerator _sourceGenerator;

        public object Current { get; private set; }

        public NullGenerator(Faker faker, NullGeneratorOptions options, IGenerator sourceGenerator)
        {
            _faker = faker;
            _options = options;
            _sourceGenerator = sourceGenerator;
        }

        public bool Next()
        {
            if (_faker.Random.Bool(_options.Weight))
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
