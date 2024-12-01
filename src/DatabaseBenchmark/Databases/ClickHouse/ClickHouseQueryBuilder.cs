using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;
using System.Text;

namespace DatabaseBenchmark.Databases.ClickHouse
{
    public class ClickHouseQueryBuilder : SqlQueryBuilder
    {
        private static readonly QueryPrimitiveOperator[] AllowedArrayOperators =
        [
            QueryPrimitiveOperator.Equals,
            QueryPrimitiveOperator.NotEquals,
            QueryPrimitiveOperator.Contains
        ];

        public ClickHouseQueryBuilder(
            Table table,
            Query query,
            ISqlParametersBuilder parametersBuilder,
            IRandomValueProvider randomValueProvider,
            IRandomPrimitives randomPrimitives) 
            : base(table, query, parametersBuilder, randomValueProvider, randomPrimitives)
        {
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
            if (column.Array && condition.Operator == QueryPrimitiveOperator.Contains)
            {
                var columnReference = BuildRegularColumnReference(condition.ColumnName);
                return $"has({columnReference}, {ParametersBuilder.Append(value, column.Type, false)})";
            }

            return base.BuildAdvancedOperatorCondition(condition, value);
        }

        protected override string BuildLimitClause()
        {
            var expression = new StringBuilder();

            if (Query.Take > 0)
            {
                expression.AppendLine($"LIMIT {Query.Take}");
            }

            if (Query.Skip > 0)
            {
                expression.AppendLine($"OFFSET {Query.Skip}");
            }

            return expression.ToString();
        }
    }
}
