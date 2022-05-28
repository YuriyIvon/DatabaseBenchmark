using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Core
{
    public class TableColumnPropertiesProvider : IColumnPropertiesProvider
    {
        private readonly Table _table;

        public TableColumnPropertiesProvider(Table table)
        {
            _table = table;
        }

        public ColumnType GetColumnType(string tableName, string columnName)
        {
            if (_table.Name != tableName)
            {
                throw new InputArgumentException($"Unknown table name \"{tableName}\"");
            }

            var column = _table.Columns.FirstOrDefault(c => c.Name == columnName);

            if (columnName == null)
            {
                throw new InputArgumentException($"Unknown column name \"{columnName}\" of the table \"{tableName}\"");
            }

            return column.Type;
        }
    }
}
