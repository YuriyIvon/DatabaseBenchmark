using DatabaseBenchmark.Common;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.CosmosDb.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.CosmosDb
{
    public class CosmosDbInsertBuilder : ICosmosDbInsertBuilder
    {
        private readonly Table _table;
        private readonly IDataSource _source;
        private readonly Dictionary<object, List<Dictionary<string, object>>> _batches = new();

        public int BatchSize { get; set; } = 1;

        public string PartitionKeyName { get; }

        public CosmosDbInsertBuilder(Table table, IDataSource source)
        {
            _table = table;
            _source = source;

            PartitionKeyName = GetPartitionKeyName(table);
        }

        public IEnumerable<IDictionary<string, object>> Build()
        {
            while (_source.Read())
            {
                var item = _table.Columns
                    .Where(c => !c.DatabaseGenerated)
                    .ToDictionary(
                        c => c.Name,
                        c => _source.GetValue(c.GetNativeType(), c.Name));

                item.Add("id", Guid.NewGuid().ToString("N"));

                if (PartitionKeyName == CosmosDbConstants.DummyPartitionKeyName)
                {
                    item.Add(PartitionKeyName, CosmosDbConstants.DummyPartitionKeyValue);
                }

                var partitionKeyValue = item[PartitionKeyName];

                if (!_batches.TryGetValue(partitionKeyValue, out var batch))
                {
                    batch = new List<Dictionary<string, object>>();
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

        private static string GetPartitionKeyName(Table table)
        {
            if (table.Columns.Count(c => c.PartitionKey) > 1)
            {
                throw new InputArgumentException("A table can't have multiple partition keys");
            }

            return table.Columns.FirstOrDefault(c => c.PartitionKey)?.Name ?? CosmosDbConstants.DummyPartitionKeyName;
        }
    }
}
