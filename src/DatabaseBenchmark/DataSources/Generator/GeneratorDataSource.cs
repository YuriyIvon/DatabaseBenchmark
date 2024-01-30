using DatabaseBenchmark.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Generators;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;

namespace DatabaseBenchmark.DataSources.Generator
{
    public sealed class GeneratorDataSource : IDataSource
    {
        private readonly GeneratorDataSourceOptions _options;
        private readonly Dictionary<string, int> _columnIndexes;
        private readonly IGenerator[] _generators;

        private object[] _currentRow;

        public GeneratorDataSource(string filePath, IDataSourceFactory dataSourceFactory, IDatabase database)
        {
            _options = JsonUtils.DeserializeFile<GeneratorDataSourceOptions>(filePath, new GeneratorOptionsConverter());

            _columnIndexes = _options.Columns
                .Select((c, i) => new { c.Name, Index = i })
                .ToDictionary(c => c.Name, c => c.Index);

            using var currentDirectoryHolder = new CurrentDirectoryHolder();
            var optionsDirectory = Path.GetDirectoryName(Path.GetFullPath(filePath));
            Directory.SetCurrentDirectory(optionsDirectory);

            var generatorFactory = new GeneratorFactory(dataSourceFactory, database);
            _generators = _options.Columns
                .Select(c => generatorFactory.Create(c.GeneratorOptions))
                .ToArray();
        }

        public object GetValue(string name)
        {
            var index = _columnIndexes[name];
            var value = _currentRow[index];

            return value;
        }

        public bool Read()
        {
            _currentRow = new object[_generators.Length];

            for (int i = 0; i < _generators.Length; i++)
            {
                var generator = _generators[i];

                if (!generator.Next())
                {
                    return false;
                }

                _currentRow[i] = generator.Current;
            }

            return true;
        }

        public void Dispose()
        {
            foreach (var generator in _generators)
            {
                if (generator is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}
