using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using System.Text;

namespace DatabaseBenchmark.Databases.Sql
{
    public class SqlInsertBuilder : ISqlInsertBuilder
    {
        public int BatchSize { get; set; } = 1;

        protected Table Table { get; }

        protected IDataSource Source { get; }

        protected ISqlParametersBuilder ParametersBuilder { get; }

        public SqlInsertBuilder(
            Table table,
            IDataSource source,
            ISqlParametersBuilder parametersBuilder)
        {
            Table = table;
            Source = source;
            ParametersBuilder = parametersBuilder;
        }

        public virtual string Build()
        {
            ParametersBuilder.Reset();

            var columns = Table.Columns.Where(c => !c.DatabaseGenerated).ToArray();
            var columnNames = columns.Select(c => c.Name).ToArray();

            int i = 0;
            var rows = new List<List<string>>();

            while (i < BatchSize && Source.Read())
            {
                var values = new List<string>();

                foreach (var column in columns)
                {
                    var value = Source.GetValue(column.GetNativeType(), column.Name);

                    if (value is double doubleValue && double.IsNaN(doubleValue))
                    {
                        value = null;
                    }

                    var valueRepresentation = ParametersBuilder.Append(value, column.Type);

                    values.Add(valueRepresentation);
                }

                rows.Add(values);

                i++;
            }

            return i > 0 ? BuildCommandText(Table.Name, columnNames, rows) : null;
        }

        protected virtual string BuildCommandText(
            string tableName,
            IEnumerable<string> columns,
            IEnumerable<IEnumerable<string>> rows)
        {
            var commandText = new StringBuilder("INSERT INTO ");
            commandText.Append(tableName);
            commandText.Append(" (");
            commandText.Append(string.Join(", ", columns));
            commandText.AppendLine(") VALUES ");

            var rowsText = string.Join(",\n",
                rows.Select(values => $"({string.Join(", ", values)})"));

            commandText.AppendLine(rowsText);

            return commandText.ToString();
        }
    }
}
