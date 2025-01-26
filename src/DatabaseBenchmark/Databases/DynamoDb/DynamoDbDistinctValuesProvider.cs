using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.DynamoDb
{
    public class DynamoDbDistinctValuesProvider : IDistinctValuesProvider
    {
        public object[] GetDistinctValues(string tableName, IValueDefinition column, bool unfoldArray)
        {
            throw new NotSupportedException("DynamoDB doesn't support distinct queries, please specify the list of possible query parameter values explicitly");
        }
    }
}
