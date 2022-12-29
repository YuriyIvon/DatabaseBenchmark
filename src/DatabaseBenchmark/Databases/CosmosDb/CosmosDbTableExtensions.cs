using DatabaseBenchmark.Common;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.CosmosDb
{
    public static class CosmosDbTableExtensions
    {
        public static string GetPartitionKeyName(this Table table)
        {
            if (table.Columns.Count(c => c.PartitionKey) > 1)
            {
                throw new InputArgumentException("A table can't have multiple partition keys");
            }

            return table.Columns.FirstOrDefault(c => c.PartitionKey)?.Name ?? CosmosDbConstants.DummyPartitionKeyName;
        }
    }
}
