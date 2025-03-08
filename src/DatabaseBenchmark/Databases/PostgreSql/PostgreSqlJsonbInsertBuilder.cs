using DatabaseBenchmark.Common;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;
using System.Text.Json;

namespace DatabaseBenchmark.Databases.PostgreSql
{
    public class PostgreSqlJsonbInsertBuilder : SqlInsertBuilder
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions = new() { Converters = { new JsonDateTimeConverter() } };

        public PostgreSqlJsonbInsertBuilder(
            Table table,
            IDataSourceReader sourceReader,
            ISqlParametersBuilder parametersBuilder,
            InsertBuilderOptions options)
            : base(table, sourceReader, parametersBuilder, options)
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
                var values = columns.Select((c, i) => ParametersBuilder.Append(sourceRow[c.Name], c.Type, c.Array)).ToList();

                var jsonbValues = Table.Columns
                    .Where(c => !c.DatabaseGenerated && c.Queryable)
                    .ToDictionary(
                        c => c.Name,
                        c => sourceRow[c.Name]);

                var jsonbParameter = ParametersBuilder.Append(
                    JsonSerializer.Serialize(jsonbValues, _jsonSerializerOptions),
                    ColumnType.Json,
                    false);

                values.Add(jsonbParameter);

                rows.Add(values);
            }

            columnNames.Add(PostgreSqlJsonbConstants.JsonbColumnName);

            if (!rows.Any())
            {
                throw new NoDataAvailableException();
            }

            return BuildCommandText(Table.Name, columnNames, rows);
        }
    }
}
