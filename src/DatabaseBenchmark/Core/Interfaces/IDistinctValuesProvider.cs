namespace DatabaseBenchmark.Core.Interfaces
{
    public interface IDistinctValuesProvider
    {
        object[] GetDistinctValues(string tableName, string columnName);
    }
}
