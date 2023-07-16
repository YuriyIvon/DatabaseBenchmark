using DatabaseBenchmark.Core.Interfaces;

namespace DatabaseBenchmark.Databases.DynamoDb
{
    public class DynamoDbDistinctValuesProvider : IDistinctValuesProvider
    {
        public object[] GetDistinctValues(string tableName, string columnName)
        {
            throw new NotImplementedException("DynamoDB doesn't support distinct queries, please specify the list of possible query parameter values explicitly");
        }
    }
}
