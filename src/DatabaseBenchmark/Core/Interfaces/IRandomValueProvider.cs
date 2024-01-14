using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Core.Interfaces
{
    public interface IRandomValueProvider
    {
        void Next();

        object GetValue(string tableName, string columnName, ValueRandomizationRule randomizationRule);

        IEnumerable<object> GetValueCollection(string tableName, string columnName, ValueRandomizationRule randomizationRule);
    }
}
