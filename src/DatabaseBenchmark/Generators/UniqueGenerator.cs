using BloomFilter;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;

namespace DatabaseBenchmark.Generators
{
    public class UniqueGenerator : IGenerator, IDisposable
    {
        private readonly UniqueGeneratorOptions _options;
        private readonly IGenerator _sourceGenerator;

        private int _maxValues = 1000000;
        private IBloomFilter _bloomFilter;

        public object Current { get; private set; }

        public bool IsBounded => _sourceGenerator.IsBounded;

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

                if (AppendFilter(_sourceGenerator.Current))
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

            _bloomFilter.Dispose();
        }

        public bool AppendFilter(object value)
        {
            _bloomFilter ??= FilterBuilder.Build(_maxValues, 0.01);

            return value switch
            {
                int intValue => _bloomFilter.Add(intValue),
                long longValue => _bloomFilter.Add(longValue),
                double doubleValue => _bloomFilter.Add(doubleValue),
                DateTime dateTimeValue => _bloomFilter.Add(dateTimeValue),
                _ => _bloomFilter.Add(value.ToString())
            };
        }

        public void SetMaxValues(int maxValues) => _maxValues = maxValues;
    }
}
