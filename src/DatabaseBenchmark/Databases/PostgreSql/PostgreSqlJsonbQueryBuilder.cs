using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;
using System.Text;

namespace DatabaseBenchmark.Databases.PostgreSql
{
    public class PostgreSqlJsonbQueryBuilder: SqlQueryBuilder
    {
        private readonly PostgreSqlJsonbQueryOptions _queryOptions;

        public PostgreSqlJsonbQueryBuilder(
            Table table,
            Query query,
            ISqlParametersBuilder parametersBuilder,
            IRandomValueProvider randomValueProvider,
            IRandomGenerator randomGenerator,
            IOptionsProvider optionsProvider) 
            : base(table, query, parametersBuilder, randomValueProvider, randomGenerator)
        {
            _queryOptions = optionsProvider.GetOptions<PostgreSqlJsonbQueryOptions>();
        }

        protected override string BuildRegularSelectColumn(string columnName) =>
            string.Join(" ", BuildRegularColumnReference(columnName), columnName);

        protected override string BuildRegularColumnReference(string columnName)
        {
            var column = GetColumn(columnName);

            if (column.Queryable)
            {
                var castType = column.Type switch
                {
                    ColumnType.Boolean => "boolean",
                    ColumnType.Guid => "uuid",
                    ColumnType.Integer => "integer",
                    ColumnType.Long => "bigint",
                    ColumnType.Double => "double",
                    _ => null
                };

                return castType != null
                    ? $"({PostgreSqlJsonbConstants.JsonbColumnName}->>'{columnName}')::{castType}"
                    : $"{PostgreSqlJsonbConstants.JsonbColumnName}->>'{columnName}'";
            }
            else
            {
                return columnName;
            }
        }

        protected override string BuildGroupCondition(QueryGroupCondition condition)
        {
            var conditions = condition.Conditions
                .Select(p => BuildCondition(p))
                .Where(p => p != null)
                .ToArray();

            if (conditions.Any())
            {
                return condition.Operator switch
                {
                    QueryGroupOperator.And => $"({string.Join(" AND ", conditions)})",
                    QueryGroupOperator.Or => $"({string.Join(" OR ", conditions)})",
                    QueryGroupOperator.Not => BuildUnaryCondition("NOT", conditions),
                    _ => throw new InputArgumentException($"Unknown group operator \"{condition.Operator}\"")
                };
            }
            else
            {
                return null;
            }
        }

        protected override string BuildPrimitiveCondition(QueryPrimitiveCondition condition)
        {
            var column = Table.Columns.FirstOrDefault(c => c.Name == condition.ColumnName);

            if (_queryOptions.UseGinOperators && column.Queryable)
            {
                if (condition.Operator == QueryPrimitiveOperator.In)
                {
                    var rawCollection = condition.RandomizeValue
                        ? RandomValueProvider.GetRandomValueCollection(Table.Name, condition.ColumnName, condition.ValueRandomizationRule)
                        : (IEnumerable<object>)condition.Value;

                    //Rewrite IN operator as a set of OR expressions
                    var orCondition = new QueryGroupCondition
                    {
                        Operator = QueryGroupOperator.Or,
                        Conditions = rawCollection.Select(v =>
                            new QueryPrimitiveCondition
                            {
                                ColumnName = condition.ColumnName,
                                Operator = QueryPrimitiveOperator.Equals,
                                Value = v
                            })
                            .ToArray()
                    };

                    return BuildGroupCondition(orCondition);
                }
                else if (condition.Operator == QueryPrimitiveOperator.Equals)
                {
                    var predicateExpression = new StringBuilder(PostgreSqlJsonbConstants.JsonbColumnName);
                    predicateExpression.Append(" @>");

                    var rawValue = condition.RandomizeValue
                        ? RandomValueProvider.GetRandomValue(Table.Name, condition.ColumnName, condition.ValueRandomizationRule)
                        : condition.Value;

                    var value = rawValue == null
                        ? "null"
                        : rawValue is string s
                            ? $"\"{EscapeString(s)}\""
                            : rawValue.ToString();

                    predicateExpression.Append($" '{{\"{column.Name}\": {value}}}'::jsonb");

                    return predicateExpression.ToString();
                }
                else
                {
                    var predicateExpression = new StringBuilder(PostgreSqlJsonbConstants.JsonbColumnName);
                    predicateExpression.Append(" @@ '$.");
                    predicateExpression.Append(condition.ColumnName);
                    predicateExpression.Append(' ');
                    predicateExpression.Append(condition.Operator switch
                    {
                        QueryPrimitiveOperator.Equals => "==",
                        QueryPrimitiveOperator.NotEquals => "!=",
                        QueryPrimitiveOperator.Greater => ">",
                        QueryPrimitiveOperator.GreaterEquals => ">=",
                        QueryPrimitiveOperator.Lower => "<",
                        QueryPrimitiveOperator.LowerEquals => "<=",
                        _ => throw new InputArgumentException($"Unknown primitive operator \"{condition.Operator}\"")
                    });
                    predicateExpression.Append(' ');

                    var rawValue = condition.RandomizeValue
                        ? RandomValueProvider.GetRandomValue(Table.Name, condition.ColumnName, condition.ValueRandomizationRule)
                        : condition.Value;

                    var value = rawValue == null
                        ? "null"
                        : rawValue is string s
                            ? $"\"{EscapeString(s)}\""
                            : rawValue.ToString();

                    predicateExpression.Append(value);
                    predicateExpression.Append('\'');

                    return predicateExpression.ToString();
                }
            }

            return base.BuildPrimitiveCondition(condition);
        }

        private static string BuildUnaryCondition(string @operator, string[] inputConditions)
        {
            if (inputConditions.Length > 1)
            {
                throw new InputArgumentException($"Operator \"{@operator}\" can have only one operand");
            }

            return $"{@operator}({inputConditions.First()})";
        }

        private static string EscapeString(string s) => s.Replace("'", "''");
    }
}
