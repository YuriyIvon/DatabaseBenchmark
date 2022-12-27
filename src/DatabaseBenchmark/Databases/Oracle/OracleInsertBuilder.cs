using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;
using System.Text;

namespace DatabaseBenchmark.Databases.Oracle
{
    public class OracleInsertBuilder : SqlInsertBuilder
    {
        private static readonly string Spacing = new(' ', 4);

        public OracleInsertBuilder(
            Table table,
            IDataSourceReader sourceReader,
            ISqlParametersBuilder parametersBuilder)
            : base(table, sourceReader, parametersBuilder)
        {
        }

        protected override string BuildCommandText(
            string tableName,
            IEnumerable<string> columns,
            IEnumerable<IEnumerable<string>> rows)
        {
            var commandText = new StringBuilder("INSERT ALL");

            foreach (var values in rows)
            {
                var columnsText = string.Join(", ", columns);
                var valuesText = string.Join(", ", values);
                commandText.AppendLine($"{Spacing}INTO {tableName}({columnsText}) VALUES ({valuesText})");
            }

            commandText.AppendLine("SELECT 1 FROM DUAL");

            return commandText.ToString();
        }
    }
}
