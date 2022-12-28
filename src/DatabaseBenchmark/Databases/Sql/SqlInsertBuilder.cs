using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;
using System.Text;

namespace DatabaseBenchmark.Databases.Sql
{
    public class SqlInsertBuilder : ISqlInsertBuilder
    {
        public int BatchSize => Options.BatchSize;

        protected Table Table { get; }

        protected IDataSourceReader SourceReader { get; }

        protected ISqlParametersBuilder ParametersBuilder { get; }

        protected InsertBuilderOptions Options { get; }

        public SqlInsertBuilder(
            Table table,
            IDataSourceReader sourceReader,
            ISqlParametersBuilder parametersBuilder,
            InsertBuilderOptions options)
        {
            Table = table;
            SourceReader = sourceReader;
            ParametersBuilder = parametersBuilder;
            Options = options;
        }

        public virtual string Build()
        {
            ParametersBuilder.Reset();

            var columns = Table.Columns.Where(c => !c.DatabaseGenerated).ToArray();
            var columnNames = columns.Select(c => c.Name).ToArray();

            var rows = new List<string[]>();

            for (var i = 0; i < BatchSize && SourceReader.ReadArray(columns, out var sourceRow); i++)
            {
                var values = columns.Select((c, i) => ParametersBuilder.Append(sourceRow[i], c.Type)).ToArray();
                rows.Add(values);
            }

            return rows.Any() ? BuildCommandText(Table.Name, columnNames, rows) : null;
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
