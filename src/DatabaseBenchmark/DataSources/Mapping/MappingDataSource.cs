﻿using DatabaseBenchmark.DataSources.Interfaces;

namespace DatabaseBenchmark.DataSources.Mapping
{
    public sealed class MappingDataSource : IDataSource
    {
        private readonly IDataSource _baseDataSource;
        private readonly Dictionary<string, string> _mappings;

        public MappingDataSource(IDataSource baseDataSource, ColumnMappingCollection mappings)
        {
            _baseDataSource = baseDataSource;
            _mappings = mappings.Columns.ToDictionary(m => m.TableColumnName, m => m.SourceColumnName);
        }

        public void Dispose() => _baseDataSource.Dispose();

        public object GetValue(Type type, string name) =>
            _baseDataSource.GetValue(type, _mappings.TryGetValue(name, out var sourceName) ? sourceName : name);

        public bool Read() => _baseDataSource.Read();
    }
}