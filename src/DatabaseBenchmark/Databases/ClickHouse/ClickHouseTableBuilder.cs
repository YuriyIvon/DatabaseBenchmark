using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Model;
using System.Text;

namespace DatabaseBenchmark.Databases.ClickHouse
{
    public class ClickHouseTableBuilder : SqlTableBuilder
    {
        private readonly IOptionsProvider _optionsProvider;

        public ClickHouseTableBuilder(IOptionsProvider optionsProvider)
        {
            _optionsProvider = optionsProvider;
        }

        public override string Build(Table table)
        {
            var options = _optionsProvider.GetOptions<ClickHouseTableOptions>();

            var query = new StringBuilder(base.Build(table));

            query.Append("ENGINE = ");
            query.AppendLine(options.Engine);
            query.Append("ORDER BY ");
            query.AppendLine(options.OrderBy);

            return query.ToString();
        }

        protected override string BuildColumnType(Column column)
        {
            var baseType = column.Type switch
            {
                ColumnType.Boolean => "UInt8",
                ColumnType.Guid => "UUID",
                ColumnType.Integer => "Int32",
                ColumnType.Long => "Int64",
                ColumnType.Double => "Float64",
                ColumnType.String => "String",
                ColumnType.Text => "String",
                ColumnType.DateTime => "DateTime64",
                _ => throw new InputArgumentException($"Unknown column type \"{column.Type}\"")
            };

            return column.Nullable ? $"Nullable({baseType})" : baseType;
        }
    }
}
