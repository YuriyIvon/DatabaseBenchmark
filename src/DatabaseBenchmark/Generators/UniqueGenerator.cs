using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;

namespace DatabaseBenchmark.Generators
{
    public class UniqueGenerator : IGenerator, IDisposable
    {
        private readonly UniqueGeneratorOptions _options;
        private readonly IGenerator _sourceGenerator;
        private readonly HashSet<object> _existingValues = [];

        public object Current { get; private set; }

        public UniqueGenerator(UniqueGeneratorOptions options, IGenerator sourceGenerator) 
        {
            _options = options;
            _sourceGenerator = sourceGenerator;
        }

        public bool Next()
        {
            for (var i = 0; i < _options.AttemptCount; i++)
            {
                _sourceGenerator.Next();

                if (_existingValues.Add(_sourceGenerator.Current))
                {
                    Current = _sourceGenerator.Current;
                    return true;
                }
            }

            return false;
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
