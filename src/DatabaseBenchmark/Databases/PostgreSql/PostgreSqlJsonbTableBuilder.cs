using DatabaseBenchmark.Common;
using DatabaseBenchmark.Model;
using System.Text;

namespace DatabaseBenchmark.Databases.PostgreSql
{
    public class PostgreSqlJsonbTableBuilder
    {
        private const string JsonbColumnName = "attributes";

        public string BuildCreateTableCommandText(Table table)
        {
            var columns = new List<string> { $"{JsonbColumnName} jsonb" };

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

            commandText.AppendLine($"CREATE INDEX {table.Name}_{JsonbColumnName}_idx ON {table.Name} USING GIN ({JsonbColumnName} jsonb_path_ops);");

            return commandText.ToString();
        }
    }
}
