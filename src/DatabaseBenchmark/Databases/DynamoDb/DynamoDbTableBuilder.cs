using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Model;
using Table = DatabaseBenchmark.Model.Table;

namespace DatabaseBenchmark.Databases.DynamoDb
{
    public class DynamoDbTableBuilder
    {
        public CreateTableRequest Build(Table table)
        {
            var partitionKey = table.GetPartitionKeyName();
            var sortKey = table.GetSortKeyName();

            if (partitionKey == null && sortKey == null)
            {
                throw new InputArgumentException("The table definition must have a partition key, a sort key, or both");
            }

            var keys = new List<KeySchemaElement>
            {
                new KeySchemaElement
                {
                    AttributeName = partitionKey ?? DynamoDbConstants.DummyPartitionKeyName,
                    KeyType = KeyType.HASH
                }
            };

            if (sortKey != null)
            {
                keys.Add(new KeySchemaElement
                {
                    AttributeName = sortKey,
                    KeyType = KeyType.RANGE
                });
            }

            return new()
            {
                AttributeDefinitions = table.Columns
                    .Where(c => c.PartitionKey || c.SortKey)
                    .Select(BuildAttribute)
                    .ToList(),
                KeySchema = keys,
                TableName = table.Name,
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 5,
                    WriteCapacityUnits = 5
                }
            };
        }

        private AttributeDefinition BuildAttribute(Column column) =>
            new ()
            {
                AttributeName = column.Name,
                AttributeType = column.Type switch
                {
                    ColumnType.Boolean => ScalarAttributeType.N,
                    ColumnType.Double => ScalarAttributeType.N,
                    ColumnType.Integer => ScalarAttributeType.N,
                    ColumnType.Long => ScalarAttributeType.N,
                    ColumnType.String => ScalarAttributeType.S,
                    ColumnType.Text => ScalarAttributeType.S,
                    ColumnType.DateTime => ScalarAttributeType.S,
                    ColumnType.Guid => ScalarAttributeType.S,
                    ColumnType.Json => ScalarAttributeType.S,
                    _ => throw new InputArgumentException($"Unknown column type \"{column.Type}\"")
                }
            };
    }
}
