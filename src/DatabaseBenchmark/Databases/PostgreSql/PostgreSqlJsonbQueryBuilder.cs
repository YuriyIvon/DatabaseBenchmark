using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Model;
using System.Text;
using System.Text.Json;

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
            IRandomPrimitives randomPrimitives,
            IOptionsProvider optionsProvider) 
            : base(table, query, parametersBuilder, randomValueProvider, randomPrimitives)
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
                .Select(BuildCondition)
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

        protected override string BuildInCondition(QueryPrimitiveCondition condition)
        {
            var column = GetColumn(condition.ColumnName);

            if (_queryOptions.UseGinOperators && column.Queryable)
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
            else
            {
                return base.BuildInCondition(condition);
            }
        }

        protected override string BuildComparisonCondition(QueryPrimitiveCondition condition, object value)
        {
            var column = GetColumn(condition.ColumnName);

            if (_queryOptions.UseGinOperators && column.Queryable)
            {
                if (condition.Operator == QueryPrimitiveOperator.Equals)
                {
                    var formattedValue = FormatValue(value);
                    return $"{PostgreSqlJsonbConstants.JsonbColumnName} @> '{{\"{column.Name}\": {formattedValue}}}'::jsonb";
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
                    predicateExpression.Append(FormatValue(value));
                    predicateExpression.Append('\'');

                    return predicateExpression.ToString();
                }
            }
            else
            {
                return base.BuildComparisonCondition(condition, value);
            }
        }

        protected override string BuildStringCondition(QueryPrimitiveCondition condition, object value)
        {
            var column = GetColumn(condition.ColumnName);

            if (_queryOptions.UseGinOperators && column.Queryable)
            {
                throw new InputArgumentException("PostgreSQL jsonb operators don't support string functions");
            }
            else
            {
                return base.BuildStringCondition(condition, value);
            }
        }

        protected override string BuildNullCondition(QueryPrimitiveCondition condition)
        {
            var column = GetColumn(condition.ColumnName);

            if (_queryOptions.UseGinOperators && column.Queryable)
            {
                var basicExpression = $"{PostgreSqlJsonbConstants.JsonbColumnName} @> '{{\"{column.Name}\": null}}'::jsonb";

                return condition.Operator switch
                {
                    QueryPrimitiveOperator.Equals => basicExpression,
                    QueryPrimitiveOperator.NotEquals => BuildNotCondition([basicExpression]),
                    _ => throw new InputArgumentException($"Primitive operator \"{condition.Operator}\" can't be used with NULL operand")
                };
            }
            else
            {
                return base.BuildNullCondition(condition);
            }
        }

        private static string BuildUnaryCondition(string @operator, string[] inputConditions)
        {
            if (inputConditions.Length > 1)
            {
                throw new InputArgumentException($"Operator \"{@operator}\" can have only one operand");
            }

            return $"{@operator}({inputConditions.First()})";
        }

        private static string FormatValue(object value) =>
            JsonSerializer.Serialize(value).Replace("'", "''");
    }
}
