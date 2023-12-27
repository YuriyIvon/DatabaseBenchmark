﻿using DatabaseBenchmark.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Generators;
using DatabaseBenchmark.Generators.Interfaces;

namespace DatabaseBenchmark.DataSources.Generator
{
    public sealed class GeneratorDataSource : IDataSource
    {
        private readonly GeneratorDataSourceOptions _options;
        private readonly Dictionary<string, int> _columnIndexes;
        private readonly IGenerator[] _generators;

        private object[] _currentRow;

        public GeneratorDataSource(string filePath, IDatabase database)
        {
            _options = JsonUtils.DeserializeFile<GeneratorDataSourceOptions>(filePath);

            _columnIndexes = _options.Columns
                .Select((c, i) => new { c.Name, Index = i })
                .ToDictionary(c => c.Name, c => c.Index);

            var generatorFactory = new GeneratorFactory(database);
            _generators = _options.Columns
                .Select(c => generatorFactory.Create(c.GeneratorOptions.Type, c.GeneratorOptions))
                .ToArray();
        }

        public object GetValue(Type type, string name)
        {
            var index = _columnIndexes[name];
            return _currentRow[index];
        }

        public bool Read()
        {
            _currentRow = _generators
                .Select(g => g.Generate())
                .ToArray();

            return true;
        }

        public void Dispose()
        {
        }
    }
}
