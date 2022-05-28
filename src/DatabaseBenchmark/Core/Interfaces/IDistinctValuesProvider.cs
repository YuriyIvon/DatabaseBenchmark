namespace DatabaseBenchmark.Core.Interfaces
{
    public interface IDistinctValuesProvider
    {
        List<object> GetDistinctValues(string tableName, string columnName);
    }
}
