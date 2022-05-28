using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Core.Interfaces
{
    public interface IColumnPropertiesProvider
    {
        ColumnType GetColumnType(string tableName, string columnName);
    }
}
