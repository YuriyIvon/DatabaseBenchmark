using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;
using System.Text;

namespace DatabaseBenchmark.Databases.PostgreSql
{
    public class PostgreSqlJsonbQueryBuilder: PostgreSqlQueryBuilder
    {
        private readonly PostgreSqlJsonbQueryOptions _queryOptions;

        public PostgreSqlJsonbQueryBuilder(
            Table table,
            Query query,
            ISqlParametersBuilder parametersBuilder,
            IRandomValueProvider randomValueProvider,
            IRandomPrimitives randomPrimitives,
            IOptionsProvider optionsProvider) 
            : base(table, query, parametersBuilder, randomValueProvider, randomPrimitives, optionsProvider)
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
                return PostgreSqlDatabaseUtils.CastExpression($"{PostgreSqlJsonbConstants.JsonbColumnName}->>'{columnName}'", column.Type);
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
                    ? RandomValueProvider.GetValueCollection(Table.Name, column, condition.ValueRandomizationRule)
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

        protected override string BuildBasicOperatorCondition(QueryPrimitiveCondition condition, object value)
        {
            var column = GetColumn(condition.ColumnName);

            if (column.Array && column.Queryable)
            {
                return BuildBasicOperatorArrayCondition(condition, value);
            }
            else if (_queryOptions.UseGinOperators && column.Queryable)
            {
                return BuildBasicOperatorScalarCondition(condition, value);
            }
            else
            {
                return base.BuildBasicOperatorCondition(condition, value);
            }
        }

        protected override string BuildAdvancedOperatorCondition(QueryPrimitiveCondition condition, object value)
        {
            var column = GetColumn(condition.ColumnName);

            if (_queryOptions.UseGinOperators && column.Queryable)
            {
                if (column.Array && condition.Operator == QueryPrimitiveOperator.Contains)
                {
                    var formattedValue = FormatValue(value);
                    return $"{PostgreSqlJsonbConstants.JsonbColumnName} @> '{{\"{column.Name}\": [{formattedValue}]}}'::jsonb";
                }
                else
                {
                    throw new InputArgumentException("PostgreSQL jsonb GIN operators don't support string functions");
                }
            }
            else
            {
                return base.BuildAdvancedOperatorCondition(condition, value);
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

        private string BuildBasicOperatorArrayCondition(QueryPrimitiveCondition condition, object value)
        {
            var conditionExpression = new StringBuilder($"{PostgreSqlJsonbConstants.JsonbColumnName}->'{condition.ColumnName}' ");

            conditionExpression.Append(condition.Operator switch
            {
                QueryPrimitiveOperator.Equals => "=",
                QueryPrimitiveOperator.NotEquals => "<>",
                _ => throw new InputArgumentException($"Unsupported array comparison operator \"{condition.Operator}\"")
            });

            conditionExpression.Append(' ');
            conditionExpression.Append(ParametersBuilder.Append(FormatValue(value), ColumnType.Json, false));

            return conditionExpression.ToString();
        }

        private static string BuildBasicOperatorScalarCondition(QueryPrimitiveCondition condition, object value)
        {
            if (condition.Operator == QueryPrimitiveOperator.Equals)
            {
                var formattedValue = FormatValue(value);
                return $"{PostgreSqlJsonbConstants.JsonbColumnName} @> '{{\"{condition.ColumnName}\": {formattedValue}}}'::jsonb";
            }
            else
            {
                var conditionExpression = new StringBuilder(PostgreSqlJsonbConstants.JsonbColumnName);
                conditionExpression.Append(" @@ '$.");
                conditionExpression.Append(condition.ColumnName);
                conditionExpression.Append(' ');
                conditionExpression.Append(condition.Operator switch
                {
                    QueryPrimitiveOperator.Equals => "==",
                    QueryPrimitiveOperator.NotEquals => "!=",
                    QueryPrimitiveOperator.Greater => ">",
                    QueryPrimitiveOperator.GreaterEquals => ">=",
                    QueryPrimitiveOperator.Lower => "<",
                    QueryPrimitiveOperator.LowerEquals => "<=",
                    _ => throw new InputArgumentException($"Unknown primitive operator \"{condition.Operator}\"")
                });
                conditionExpression.Append(' ');
                conditionExpression.Append(FormatValue(value));
                conditionExpression.Append('\'');

                return conditionExpression.ToString();
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
    }
}
