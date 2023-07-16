using DatabaseBenchmark.Common;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.DynamoDb
{
    public static class DynamoDbTableExtensions
    {
        public static string GetPartitionKeyName(this Table table)
        {
            if (table.Columns.Count(c => c.PartitionKey) > 1)
            {
                throw new InputArgumentException("A table can have only one partition key");
            }

            return table.Columns.FirstOrDefault(c => c.PartitionKey)?.Name;
        }

        public static string GetSortKeyName(this Table table)
        {
            if (table.Columns.Count(c => c.SortKey) > 1)
            {
                throw new InputArgumentException("A table can have only one sort key");
            }

            return table.Columns.FirstOrDefault(c => c.SortKey)?.Name;
        }
    }
}
