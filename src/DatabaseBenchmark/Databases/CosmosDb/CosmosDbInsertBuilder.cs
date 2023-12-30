using DatabaseBenchmark.Common;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.CosmosDb.Interfaces;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.CosmosDb
{
    public class CosmosDbInsertBuilder : ICosmosDbInsertBuilder
    {
        private readonly Table _table;
        private readonly IDataSourceReader _sourceReader;
        private readonly InsertBuilderOptions _options;
        private readonly Dictionary<object, List<IDictionary<string, object>>> _batches = [];

        public int BatchSize => _options.BatchSize;

        public string PartitionKeyName { get; }

        public CosmosDbInsertBuilder(
            Table table,
            IDataSourceReader sourceReader,
            InsertBuilderOptions options)
        {
            _table = table;
            _sourceReader = sourceReader;
            _options = options;

            PartitionKeyName = table.GetPartitionKeyName();
        }

        public IEnumerable<IDictionary<string, object>> Build()
        {
            while (_sourceReader.ReadDictionary(_table.Columns, out var item))
            {
                item.Add("id", Guid.NewGuid().ToString("N"));

                if (PartitionKeyName == CosmosDbConstants.DummyPartitionKeyName)
                {
                    item.Add(PartitionKeyName, CosmosDbConstants.DummyPartitionKeyValue);
                }

                var partitionKeyValue = item[PartitionKeyName];

                if (!_batches.TryGetValue(partitionKeyValue, out var batch))
                {
                    batch = new List<IDictionary<string, object>>();
                    _batches.Add(partitionKeyValue, batch);
                }

                batch.Add(item);

                if (batch.Count >= BatchSize)
                {
                    _batches.Remove(partitionKeyValue);
                    return batch;
                }
            }

            if (_batches.Any())
            {
                var batch = _batches.FirstOrDefault();
                _batches.Remove(batch.Key);
                return batch.Value;
            }

            return Enumerable.Empty<IDictionary<string, object>>();
        }
    }
}
