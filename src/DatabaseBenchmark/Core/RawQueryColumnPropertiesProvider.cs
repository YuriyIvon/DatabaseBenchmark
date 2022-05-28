using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Core
{
    public class RawQueryColumnPropertiesProvider : IColumnPropertiesProvider
    {
        private readonly RawQuery _query;

        public RawQueryColumnPropertiesProvider(RawQuery query)
        {
            _query = query;
        }

        public ColumnType GetColumnType(string tableName, string columnName)
        {
            var parameter = _query.Parameters.FirstOrDefault(p => p.Name == columnName);

            if (parameter == null)
            {
                throw new InputArgumentException($"Unknown raw query parameter \"{columnName}\"");
            }

            return parameter.Type;
        }
    }
}
