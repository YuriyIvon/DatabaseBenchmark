using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Core.Interfaces
{
    public interface IDistinctValuesProvider
    {
        object[] GetDistinctValues(string tableName, IValueDefinition column, bool unfoldArray);
    }
}
