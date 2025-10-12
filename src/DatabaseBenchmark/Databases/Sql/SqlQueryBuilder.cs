using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;
using System.Text;

namespace DatabaseBenchmark.Databases.Sql
{
    public class SqlQueryBuilder : ISqlQueryBuilder
    {
        private static readonly string Spacing = new(' ', 4);

        protected Table Table { get; }

        protected Query Query { get; }

        protected ISqlParametersBuilder ParametersBuilder { get; }

        protected IRandomValueProvider RandomValueProvider { get; }

        protected IRandomPrimitives RandomPrimitives { get; }

        public SqlQueryBuilder(
            Table table,
            Query query,
            ISqlParametersBuilder parametersBuilder,
            IRandomValueProvider randomValueProvider,
            IRandomPrimitives randomPrimitives)
        {
            Table = table;
            Query = query;
            ParametersBuilder = parametersBuilder;
            RandomValueProvider = randomValueProvider;
            RandomPrimitives = randomPrimitives;
        }

        public string Build()
        {
            ParametersBuilder.Reset();
            RandomValueProvider?.Next();

            var query = new StringBuilder();
            query.AppendLine("SELECT");

            query.Append(BuildSelectClause());

            query.AppendLine("FROM");
            query.Append(Spacing);
            query.AppendLine(Table.Name);

            if (Query.Condition != null)
            {
                var expression = BuildCondition(Query.Condition);
                if (expression != null)
                {
                    query.AppendLine("WHERE");
                    query.Append(Spacing);
                    query.AppendLine(expression);
                }
            }

            if (Query.Aggregate != null)
            {
                query.AppendLine("GROUP BY");
                query.Append(Spacing);

                query.AppendLine(BuildGroupByClause());
            }

            if (Query.Sort != null)
            {
                query.AppendLine("ORDER BY");
                query.Append(Spacing);

                query.AppendLine(BuildOrderByClause());
            }

            var limit = BuildLimitClause();
            if (!string.IsNullOrEmpty(limit))
            {
                query.Append(limit);
            }

            return query.ToString();
        }

        protected virtual string BuildRegularSelectColumn(string columnName) => BuildRegularColumnReference(columnName);

        protected virtual string BuildRegularColumnReference(string columnName) => columnName;

        protected virtual string BuildAggregateSelectColumn(QueryAggregateColumn column)
        {
            var sourceColumnExpression = string.IsNullOrEmpty(column.SourceColumnName)
                ? "1"
                : BuildRegularColumnReference(column.SourceColumnName);

            var aggregateFunction = column.Function switch
            {
                QueryAggregateFunction.Average => "AVG",
                QueryAggregateFunction.Count => "COUNT",
                QueryAggregateFunction.DistinctCount => "COUNT",
                QueryAggregateFunction.Max => "MAX",
                QueryAggregateFunction.Min => "MIN",
                QueryAggregateFunction.Sum => "SUM",
                _ => throw new InputArgumentException($"Unknown aggregate function \"{column.Function}\"")
            };

            string sourceColumnModifier = column.Function == QueryAggregateFunction.DistinctCount ? "DISTINCT" : "";
            return $"{aggregateFunction}({sourceColumnModifier}{sourceColumnExpression}) {column.ResultColumnName}";
        }

        protected virtual string BuildSelectClause()
        {
            var selectClause = new StringBuilder();
            var columns = new List<string>();

            if (Query.Columns != null)
            {
                columns.AddRange(Query.Columns.Select(BuildRegularSelectColumn));
            }

            if (Query.Aggregate != null)
            {
                columns.AddRange(Query.Aggregate.ResultColumns.Select(BuildAggregateSelectColumn));
            }

            if (columns.Any())
            {
                if (Query.Distinct)
                {
                    selectClause.AppendLine("DISTINCT");
                }

                var expression = string.Join("," + Environment.NewLine, columns.Select(c => Spacing + c));
                selectClause.AppendLine(expression);
            }
            else
            {
                if (Query.Distinct)
                {
                    throw new InputArgumentException("Columns must be explicitly specified if the distinct flag is set");
                }

                selectClause.Append(Spacing);
                selectClause.AppendLine("*");
            }

            return selectClause.ToString();
        }

        protected virtual string BuildCondition(IQueryCondition condition)
        {
            if (condition.RandomizeInclusion && RandomPrimitives.GetRandomBoolean())
            {
                return null;
            }
            else
            {
                return condition switch
                {
                    QueryGroupCondition groupCondition => BuildGroupCondition(groupCondition),
                    QueryPrimitiveCondition primitiveCondition => BuildPrimitiveCondition(primitiveCondition),
                    _ => throw new InputArgumentException($"Unknown condition type \"{condition.GetType()}\"")
                };
            }
        }

        protected virtual string BuildNullCondition(QueryPrimitiveCondition condition)
        {
            var columnExpression = BuildRegularColumnReference(condition.ColumnName);

            return condition.Operator switch
            {
                QueryPrimitiveOperator.Equals => $"{columnExpression} IS NULL",
                QueryPrimitiveOperator.NotEquals => $"{columnExpression} IS NOT NULL",
                _ => throw new InputArgumentException($"Primitive operator \"{condition.Operator}\" can't be used with NULL operand")
            };
        }

        protected virtual string BuildGroupCondition(QueryGroupCondition condition)
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
                    QueryGroupOperator.Not => BuildNotCondition(conditions),
                    _ => throw new InputArgumentException($"Unknown group operator \"{condition.Operator}\"")
                };
            }
            else
            {
                return null;
            }
        }

        protected virtual string BuildNotCondition(string[] inputConditions)
        {
            if (inputConditions.Length > 1)
            {
                throw new InputArgumentException($"Operator \"Not\" can have only one operand");
            }

            return $"NOT ({inputConditions.First()})";
        }

        protected virtual string BuildPrimitiveCondition(QueryPrimitiveCondition condition)
        {
            if (condition.Operator == QueryPrimitiveOperator.In)
            {
                return BuildInCondition(condition);
            }
            else
            {
                var column = GetColumn(condition.ColumnName);

                var value = condition.RandomizeValue
                    ? RandomValueProvider.GetValue(Table.Name, column, condition.ValueRandomizationRule)
                    : condition.Value;

                if (value != null)
                {
                    if (condition.Operator == QueryPrimitiveOperator.Contains || condition.Operator == QueryPrimitiveOperator.StartsWith)
                    {
                        return BuildAdvancedOperatorCondition(condition, value);
                    }
                    else
                    {
                        return BuildBasicOperatorCondition(condition, value);
                    }
                }
                else
                {
                    return BuildNullCondition(condition);
                }
            }
        }

        protected virtual string BuildInCondition(QueryPrimitiveCondition condition)
        {
            var column = GetColumn(condition.ColumnName);

            var conditionExpression = new StringBuilder(BuildRegularColumnReference(condition.ColumnName));
            conditionExpression.Append(' ');

            var rawCollection = condition.RandomizeValue
                ? RandomValueProvider.GetValueCollection(Table.Name, column, condition.ValueRandomizationRule)
                : (IEnumerable<object>)condition.Value;

            if (rawCollection == null)
            {
                throw new InputArgumentException($"Primitive operator \"{condition.Operator}\" can't be used with NULL operand");
            }

            conditionExpression.Append("IN (");
            conditionExpression.Append(string.Join(", ", rawCollection.Select(v => ParametersBuilder.Append(v, column.Type, false))));
            conditionExpression.Append(')');

            return conditionExpression.ToString();
        }

        protected virtual string BuildAdvancedOperatorCondition(QueryPrimitiveCondition condition, object value)
        {
            var column = GetColumn(condition.ColumnName);
            var columnReference = BuildRegularColumnReference(condition.ColumnName);

            var pattern = condition.Operator switch
            {
                QueryPrimitiveOperator.Contains => $"%{value}%",
                QueryPrimitiveOperator.StartsWith => $"{value}%",
                _ => throw new InputArgumentException($"Unknown string operator \"{condition.Operator}\"")
            };

            return $"{columnReference} LIKE {ParametersBuilder.Append(pattern, column.Type, false)}";
        }

        protected virtual string BuildBasicOperatorCondition(QueryPrimitiveCondition condition, object value)
        {
            var column = GetColumn(condition.ColumnName);
            var columnReference = BuildRegularColumnReference(condition.ColumnName);

            var conditionExpression = new StringBuilder(columnReference);
            conditionExpression.Append(' ');

            conditionExpression.Append(condition.Operator switch
            {
                QueryPrimitiveOperator.Equals => "=",
                QueryPrimitiveOperator.NotEquals => "<>",
                QueryPrimitiveOperator.Greater => ">",
                QueryPrimitiveOperator.GreaterEquals => ">=",
                QueryPrimitiveOperator.Lower => "<",
                QueryPrimitiveOperator.LowerEquals => "<=",
                _ => throw new InputArgumentException($"Unknown comparison operator \"{condition.Operator}\"")
            });

            conditionExpression.Append(' ');
            conditionExpression.Append(ParametersBuilder.Append(value, column.Type, column.Array));

            return conditionExpression.ToString();
        }

        protected virtual string BuildGroupByClause() =>
            string.Join(", ", Query.Aggregate.GroupColumnNames.Select(BuildRegularColumnReference));

        protected virtual string BuildOrderByClause() =>
            string.Join(", ",
                Query.Sort.Select(x =>
                {
                    var direction = x.Direction == QuerySortDirection.Ascending ? "ASC" : "DESC";
                    return $"{BuildRegularColumnReference(x.ColumnName)} {direction}";
                }));

        protected virtual string BuildLimitClause()
        {
            var expression = new StringBuilder();

            if (Query.Skip > 0 || Query.Take > 0)
            {
                expression.AppendLine($"OFFSET {ParametersBuilder.Append(Query.Skip, ColumnType.Integer, false)} ROWS");
            }

            if (Query.Take > 0)
            {
                expression.AppendLine($"FETCH NEXT {ParametersBuilder.Append(Query.Take, ColumnType.Integer, false)} ROWS ONLY");
            }

            return expression.ToString();
        }

        protected Column GetColumn(string columnName)
        {
            var column = Table.Columns.FirstOrDefault(c => c.Name == columnName);
            return column ?? throw new InputArgumentException($"Unknown column \"{columnName}\"");
        }
    }
}
