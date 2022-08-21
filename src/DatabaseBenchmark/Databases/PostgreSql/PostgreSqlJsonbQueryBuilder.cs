using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Sql;
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
            SqlParametersBuilder parametersBuilder,
            IRandomValueProvider randomValueProvider,
            IOptionsProvider optionsProvider) : base(table, query, parametersBuilder, randomValueProvider)
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
                    ? $"(attributes->>'{columnName}')::{castType}"
                    : $"attributes->>'{columnName}'";
            }
            else
            {
                return columnName;
            }
        }

        protected override string BuildGroupCondition(QueryGroupCondition predicate)
        {
            var predicates = predicate.Conditions.Select(p => BuildCondition(p)).ToArray();

            if (!predicates.Any())
            {
                throw new InputArgumentException("No predicates in a group");
            }

            return predicate.Operator switch
            {
                QueryGroupOperator.And => $"({string.Join(" AND ", predicates)})",
                QueryGroupOperator.Or => $"({string.Join(" OR ", predicates)})",
                QueryGroupOperator.Not => BuildUnaryCondition("NOT", predicates),
                _ => throw new InputArgumentException($"Unknown group operator \"{predicate.Operator}\"")
            };
        }

        protected override string BuildPrimitiveCondition(QueryPrimitiveCondition predicate)
        {
            var column = Table.Columns.FirstOrDefault(c => c.Name == predicate.ColumnName);

            if (_queryOptions.UseGinOperators && column.Queryable)
            {
                if (predicate.Operator == QueryPrimitiveOperator.In)
                {
                    var rawCollection = predicate.RandomizeValue
                        ? RandomValueProvider.GetRandomValueCollection(Table.Name, predicate.ColumnName, predicate.ValueRandomizationRule)
                        : (IEnumerable<object>)predicate.Value;

                    //Rewrite IN operator as a set of OR expressions
                    var orCondition = new QueryGroupCondition
                    {
                        Operator = QueryGroupOperator.Or,
                        Conditions = rawCollection.Select(v =>
                            new QueryPrimitiveCondition
                            {
                                ColumnName = predicate.ColumnName,
                                Operator = QueryPrimitiveOperator.Equals,
                                Value = v
                            })
                            .ToArray()
                    };

                    return BuildGroupCondition(orCondition);
                }
                else if (predicate.Operator == QueryPrimitiveOperator.Equals)
                {
                    var predicateExpression = new StringBuilder("attributes @>");

                    var rawValue = predicate.RandomizeValue
                        ? RandomValueProvider.GetRandomValue(Table.Name, predicate.ColumnName, predicate.ValueRandomizationRule)
                        : predicate.Value;

                    var value = rawValue == null
                        ? "null"
                        : rawValue is string s
                            ? $"\"{EscapeString(s)}\""
                            : rawValue.ToString();

                    predicateExpression.Append($" '{{\"{column.Name}\": {value}}}'::jsonb");

                    return predicateExpression.ToString();
                }
            }

            return base.BuildPrimitiveCondition(predicate);
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
