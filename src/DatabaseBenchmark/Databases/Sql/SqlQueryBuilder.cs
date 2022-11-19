using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
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

        protected SqlParametersBuilder ParametersBuilder { get; }

        protected IRandomValueProvider RandomValueProvider { get; }

        protected IRandomGenerator RandomGenerator { get; }

        public SqlQueryBuilder(
            Table table,
            Query query,
            SqlParametersBuilder parametersBuilder,
            IRandomValueProvider randomValueProvider,
            IRandomGenerator randomGenerator)
        {
            Table = table;
            Query = query;
            ParametersBuilder = parametersBuilder;
            RandomValueProvider = randomValueProvider;
            RandomGenerator = randomGenerator;
        }

        public string Build()
        {
            ParametersBuilder.Reset();

            var query = new StringBuilder();
            query.AppendLine("SELECT");

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
                var expression = string.Join("," + Environment.NewLine, columns.Select(c => Spacing + c));
                query.AppendLine(expression);
            }
            else
            {
                query.Append(Spacing);
                query.AppendLine("*");
            }

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

                query.AppendLine(string.Join(", ", Query.Aggregate.GroupColumnNames.Select(BuildRegularColumnReference)));
            }

            if (Query.Sort != null)
            {
                query.AppendLine("ORDER BY");
                query.Append(Spacing);

                query.AppendLine(
                    string.Join(", ",
                        Query.Sort.Select(x =>
                        {
                            var direction = x.Direction == QuerySortDirection.Ascending ? "ASC" : "DESC";
                            return $"{BuildRegularColumnReference(x.ColumnName)} {direction}";
                        })));
            }

            var limit = BuildLimit();
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

        protected virtual string BuildCondition(IQueryCondition condition)
        {
            if (condition.RandomizeInclusion && RandomGenerator.GetRandomBoolean())
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
                throw new InputArgumentException($"Operator NOT can have only one operand");
            }

            return $"NOT ({inputConditions.First()})";
        }

        protected virtual string BuildPrimitiveCondition(QueryPrimitiveCondition condition)
        {
            if (condition.Operator == QueryPrimitiveOperator.In)
            {
                var conditionExpression = new StringBuilder(BuildRegularColumnReference(condition.ColumnName));
                conditionExpression.Append(' ');

                var rawCollection = condition.RandomizeValue
                    ? RandomValueProvider.GetRandomValueCollection(Table.Name, condition.ColumnName, condition.ValueRandomizationRule)
                    : (IEnumerable<object>)condition.Value;

                if (rawCollection == null)
                {
                    throw new InputArgumentException($"Primitive operator \"{condition.Operator}\" can't be used with NULL operand");
                }

                conditionExpression.Append("IN (");
                conditionExpression.Append(string.Join(", ", rawCollection.Select(ParametersBuilder.Append)));
                conditionExpression.Append(')');

                return conditionExpression.ToString();
            }
            else
            {
                var conditionExpression = new StringBuilder(BuildRegularColumnReference(condition.ColumnName));
                conditionExpression.Append(' ');

                var rawValue = condition.RandomizeValue
                    ? RandomValueProvider.GetRandomValue(Table.Name, condition.ColumnName, condition.ValueRandomizationRule)
                    : condition.Value;

                if (rawValue != null)
                {
                    conditionExpression.Append(condition.Operator switch
                    {
                        QueryPrimitiveOperator.Equals => "=",
                        QueryPrimitiveOperator.NotEquals => "<>",
                        QueryPrimitiveOperator.Greater => ">",
                        QueryPrimitiveOperator.GreaterEquals => ">=",
                        QueryPrimitiveOperator.Lower => "<",
                        QueryPrimitiveOperator.LowerEquals => "<=",
                        QueryPrimitiveOperator.Contains => "LIKE",
                        QueryPrimitiveOperator.StartsWith => "LIKE",
                        _ => throw new InputArgumentException($"Unknown primitive operator \"{condition.Operator}\"")
                    });

                    var value = condition.Operator switch
                    {
                        QueryPrimitiveOperator.Contains => $"%{rawValue}%",
                        QueryPrimitiveOperator.StartsWith => $"{rawValue}%",
                        _ => rawValue
                    };

                    conditionExpression.Append(' ');
                    conditionExpression.Append(ParametersBuilder.Append(value));

                    return conditionExpression.ToString();
                }
                else
                {
                    return BuildNullCondition(condition);
                }
            }
        }

        protected virtual string BuildLimit()
        {
            var expression = new StringBuilder();

            if (Query.Skip > 0 || Query.Take > 0)
            {
                expression.AppendLine($"OFFSET {ParametersBuilder.Append(Query.Skip)} ROWS");
            }

            if (Query.Take > 0)
            {
                expression.AppendLine($"FETCH NEXT {ParametersBuilder.Append(Query.Take)} ROWS ONLY");
            }

            return expression.ToString();
        }

        protected Column GetColumn(string columnName)
        {
            var column = Table.Columns.FirstOrDefault(c => c.Name == columnName);
            if (column == null)
            {
                throw new InputArgumentException($"Unknown column \"{columnName}\"");
            }

            return column;
        }
    }
}
