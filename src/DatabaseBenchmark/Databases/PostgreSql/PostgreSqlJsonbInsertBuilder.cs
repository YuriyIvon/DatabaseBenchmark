using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using System.Text.Json;

namespace DatabaseBenchmark.Databases.PostgreSql
{
    public class PostgreSqlJsonbInsertBuilder : SqlInsertBuilder
    {
        public PostgreSqlJsonbInsertBuilder(Table table,
            IDataSource dataSource,
            ISqlParametersBuilder parametersBuilder)
            : base(table, dataSource, parametersBuilder)
        {
        }

        public override string Build()
        {
            ParametersBuilder.Reset();

            var columns = Table.Columns.Where(c => !c.DatabaseGenerated && !c.Queryable).ToArray();
            var columnNames = columns.Select(c => c.Name).ToList();
            columnNames.Add(PostgreSqlJsonbConstants.JsonbColumnName);

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

                var jsonbValues = Table.Columns
                    .Where(c => c.Queryable)
                    .ToDictionary(
                        c => c.Name,
                        c => Source.GetValue(c.GetNativeType(), c.Name));

                var jsonbParameter = ParametersBuilder.Append(
                    JsonSerializer.Serialize(jsonbValues),
                    ColumnType.Json);

                values.Add(jsonbParameter);

                rows.Add(values);

                i++;
            }

            return i > 0 ? BuildCommandText(Table.Name, columnNames, rows) : null;
        }
    }
}
