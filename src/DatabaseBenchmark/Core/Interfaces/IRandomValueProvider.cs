using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Core.Interfaces
{
    public interface IRandomValueProvider
    {
        void Next();

        object GetValue(string tableName, IValueDefinition valueDefinition, ValueRandomizationRule randomizationRule);

        IEnumerable<object> GetValueCollection(string tableName, IValueDefinition valueDefinition, ValueRandomizationRule randomizationRule);
    }
}
