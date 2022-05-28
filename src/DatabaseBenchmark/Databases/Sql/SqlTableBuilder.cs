using DatabaseBenchmark.Model;
using System.Text;

namespace DatabaseBenchmark.Databases.Sql
{
    public abstract class SqlTableBuilder
    {
        public virtual string Build(Table table)
        {
            var commandText = new StringBuilder("CREATE TABLE ");
            commandText.Append(table.Name);
            commandText.AppendLine("(");
            var columns = string.Join(",\n", table.Columns.Select(c => $"{c.Name} {BuildColumnType(c)}"));
            commandText.Append(columns);
            commandText.AppendLine(")");

            return commandText.ToString();
        }

        protected abstract string BuildColumnType(Column column);
    }
}
