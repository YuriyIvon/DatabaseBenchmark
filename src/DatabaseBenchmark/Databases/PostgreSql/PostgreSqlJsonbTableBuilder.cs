using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Model;
using System.Text;

namespace DatabaseBenchmark.Databases.PostgreSql
{
    public class PostgreSqlJsonbTableBuilder
    {
        private readonly IOptionsProvider _optionsProvider;

        public PostgreSqlJsonbTableBuilder(IOptionsProvider optionsProvider)
        {
            _optionsProvider = optionsProvider;
        }

        public string BuildCreateTableCommandText(Table table)
        {
            var options = _optionsProvider.GetOptions<PostgreSqlJsonbTableOptions>();

            var columns = new List<string> { $"{PostgreSqlJsonbConstants.JsonbColumnName} jsonb" };

            if (table.Columns.Any(c => c.Queryable && c.DatabaseGenerated))
            {
                throw new InputArgumentException("JSONB columns can't be database-generated");
            }

            columns.AddRange(table.Columns
                .Where(c => !c.Queryable)
                .Select(c => $"{c.Name} {PostgreSqlDatabaseUtils.BuildColumnType(c)}"));

            var commandText = new StringBuilder("CREATE TABLE ");
            commandText.Append(table.Name);
            commandText.AppendLine("(");
            commandText.Append(string.Join(",\n", columns));
            commandText.AppendLine(");");
            commandText.AppendLine();

            if (options.CreateGinIndex)
            {
                commandText.AppendLine($"CREATE INDEX {table.Name}_{PostgreSqlJsonbConstants.JsonbColumnName}_idx ON {table.Name} USING GIN ({PostgreSqlJsonbConstants.JsonbColumnName} jsonb_path_ops);");
            }

            return commandText.ToString();
        }
    }
}
