using Amazon.DynamoDBv2.Model;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.DynamoDb.Interfaces;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.DynamoDb
{
    public class DynamoDbInsertBuilder : IDynamoDbInsertBuilder
    {
        private readonly Table _table;
        private readonly IDataSourceReader _sourceReader;
        private readonly InsertBuilderOptions _options;

        public int BatchSize => _options.BatchSize;

        public string PartitionKeyName { get; }

        public DynamoDbInsertBuilder(
            Table table,
            IDataSourceReader sourceReader,
            InsertBuilderOptions options)
        {
            _table = table;
            _sourceReader = sourceReader;
            _options = options;

            PartitionKeyName = table.GetPartitionKeyName();
        }

        public IEnumerable<Dictionary<string, AttributeValue>> Build()
        {
            var batch = new List<Dictionary<string, AttributeValue>>();

            while (batch.Count < BatchSize && _sourceReader.ReadArray(_table.Columns, out var values))
            {
                var item = _table.Columns
                    .Where(c => !c.DatabaseGenerated)
                    .Select((c, i) => (c.Name, Value: DynamoDbAttributeValueUtils.ToAttributeValue(c.Type, values[i])))
                    .ToDictionary(t => t.Name, t => t.Value);

                if (PartitionKeyName == null)
                {
                    item.Add(DynamoDbConstants.DummyPartitionKeyName, new AttributeValue(DynamoDbConstants.DummyPartitionKeyValue));
                }

                batch.Add(item);
            }

            if (!batch.Any())
            {
                throw new NoDataAvailableException();
            }

            return batch;
        }
    }
}
