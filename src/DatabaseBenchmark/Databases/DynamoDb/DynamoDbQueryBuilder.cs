using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.DynamoDb
{
    public class DynamoDbQueryBuilder : SqlQueryBuilder
    {
        private static readonly QueryPrimitiveOperator[] AllowedArrayOperators =
        [
            QueryPrimitiveOperator.Equals,
            QueryPrimitiveOperator.NotEquals,
            QueryPrimitiveOperator.Contains
        ];

        public DynamoDbQueryBuilder(
            Table table,
            Query query,
            ISqlParametersBuilder parametersBuilder,
            IRandomValueProvider randomValueProvider,
            IRandomPrimitives randomPrimitives)
            : base(table, query, parametersBuilder, randomValueProvider, randomPrimitives)
        {
        }

        protected override string BuildAggregateSelectColumn(QueryAggregateColumn column) =>
            throw new InputArgumentException("DynamoDB doesn't support aggregations");

        protected override string BuildSelectClause()
        {
            if (Query.Distinct)
            {
                throw new InputArgumentException("DynamoDB doesn't support distinct queries");
            }

            return base.BuildSelectClause();
        }

        protected override string BuildPrimitiveCondition(QueryPrimitiveCondition condition)
        {
            var column = GetColumn(condition.ColumnName);
            if (column.Array && !AllowedArrayOperators.Contains(condition.Operator))
            {
                throw new InputArgumentException($"Primitive operator \"{condition.Operator}\" is not supported for array columns");
            }

            return base.BuildPrimitiveCondition(condition);
        }

        protected override string BuildAdvancedOperatorCondition(QueryPrimitiveCondition condition, object value)
        {
            var column = GetColumn(condition.ColumnName);
            var columnReference = BuildRegularColumnReference(condition.ColumnName);

            var function = condition.Operator switch
            {
                QueryPrimitiveOperator.Contains => $"contains",
                QueryPrimitiveOperator.StartsWith => $"begins_with",
                _ => throw new InputArgumentException($"Unknown string operator \"{condition.Operator}\"")
            };

            return $"{function}({columnReference}, {ParametersBuilder.Append(value, column.Type, false)})";
        }

        protected override string BuildNullCondition(QueryPrimitiveCondition condition)
        {
            var columnReference = BuildRegularColumnReference(condition.ColumnName);

            return condition.Operator switch
            {
                QueryPrimitiveOperator.Equals => $"attribute_not_exists({columnReference})",
                QueryPrimitiveOperator.NotEquals => $"attribute_exists({columnReference})",
                _ => throw new InputArgumentException($"Primitive operator \"{condition.Operator}\" can't be used with NULL operand")
            };
        }

        protected override string BuildLimitClause()
        {
            if (Query.Take > 0 || Query.Skip > 0)
            {
                throw new InputArgumentException("DynamoDB doesn't support offsets");
            }

            return null;
        }
    }
}
