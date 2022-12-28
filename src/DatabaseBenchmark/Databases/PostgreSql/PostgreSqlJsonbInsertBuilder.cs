using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;
using System.Text.Json;

namespace DatabaseBenchmark.Databases.PostgreSql
{
    public class PostgreSqlJsonbInsertBuilder : SqlInsertBuilder
    {
        public PostgreSqlJsonbInsertBuilder(Table table,
            IDataSourceReader sourceReader,
            ISqlParametersBuilder parametersBuilder)
            : base(table, sourceReader, parametersBuilder)
        {
        }

        public override string Build()
        {
            ParametersBuilder.Reset();

            var columns = Table.Columns.Where(c => !c.DatabaseGenerated && !c.Queryable).ToArray();
            var columnNames = columns.Select(c => c.Name).ToList();

            var rows = new List<List<string>>();

            for (var i = 0; i < BatchSize && SourceReader.ReadDictionary(Table.Columns, out var sourceRow); i++)
            {
                var values = columns.Select((c, i) => ParametersBuilder.Append(sourceRow[c.Name], c.Type)).ToList();

                var jsonbValues = Table.Columns
                    .Where(c => !c.DatabaseGenerated && c.Queryable)
                    .ToDictionary(
                        c => c.Name,
                        c => sourceRow[c.Name]);

                var jsonbParameter = ParametersBuilder.Append(
                    JsonSerializer.Serialize(jsonbValues),
                    ColumnType.Json);

                values.Add(jsonbParameter);

                rows.Add(values);
            }

            columnNames.Add(PostgreSqlJsonbConstants.JsonbColumnName);

            return rows.Any() ? BuildCommandText(Table.Name, columnNames, rows) : null;
        }
    }
}
