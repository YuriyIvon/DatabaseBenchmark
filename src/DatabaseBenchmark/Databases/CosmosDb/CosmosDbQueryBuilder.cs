using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;
using System.Text;

namespace DatabaseBenchmark.Databases.CosmosDb
{
    public class CosmosDbQueryBuilder : SqlQueryBuilder
    {
        private static readonly QueryPrimitiveOperator[] AllowedArrayOperators =
        [
            QueryPrimitiveOperator.Equals,
            QueryPrimitiveOperator.NotEquals,
            QueryPrimitiveOperator.Contains
        ];

        public CosmosDbQueryBuilder(
            Table table,
            Query query,
            ISqlParametersBuilder parametersBuilder,
            IRandomValueProvider randomValueProvider,
            IRandomPrimitives randomPrimitives) 
            : base(table, query, parametersBuilder, randomValueProvider, randomPrimitives)
        {
        }

        protected override string BuildRegularColumnReference(string columnName) => $"{Table.Name}.{columnName}";

        protected override string BuildPrimitiveCondition(QueryPrimitiveCondition condition)
        {
            var column = GetColumn(condition.ColumnName);
            if (column.Array && !AllowedArrayOperators.Contains(condition.Operator))
            {
                throw new InputArgumentException($"Primitive operator \"{condition.Operator}\" is not supported for array columns");
            }

            return base.BuildPrimitiveCondition(condition);
        }

        protected override string BuildNullCondition(QueryPrimitiveCondition condition)
        {
            var columnExpression = BuildRegularColumnReference(condition.ColumnName);

            return condition.Operator switch
            {
                QueryPrimitiveOperator.Equals => $"IS_NULL({columnExpression})",
                QueryPrimitiveOperator.NotEquals => $"NOT IS_NULL({columnExpression})",
                _ => throw new InputArgumentException($"Primitive operator \"{condition.Operator}\" can't be used with NULL operand")
            };
        }

        protected override string BuildAdvancedOperatorCondition(QueryPrimitiveCondition condition, object value)
        {
            var column = GetColumn(condition.ColumnName);
            if (column.Array && condition.Operator == QueryPrimitiveOperator.Contains)
            {
                var columnReference = BuildRegularColumnReference(condition.ColumnName);
                return $"ARRAY_CONTAINS({columnReference}, {ParametersBuilder.Append(value, column.Type, false)})";
            }

            return base.BuildAdvancedOperatorCondition(condition, value);
        }

        protected override string BuildLimitClause()
        {
            var expression = new StringBuilder();

            if (Query.Skip > 0 || Query.Take > 0)
            {
                expression.AppendLine($"OFFSET {ParametersBuilder.Append(Query.Skip, ColumnType.Integer, false)}");
            }

            if (Query.Take > 0)
            {
                expression.AppendLine($"LIMIT {ParametersBuilder.Append(Query.Take, ColumnType.Integer, false)}");
            }

            return expression.ToString();
        }
    }
}
