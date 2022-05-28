using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Core.Interfaces
{
    public interface IRandomValueProvider
    {
        object GetRandomValue(string tableName, string columnName, ValueRandomizationRule randomizationRule);

        IEnumerable<object> GetRandomValueCollection(string tableName, string columnName, ValueRandomizationRule randomizationRule);
    }
}
