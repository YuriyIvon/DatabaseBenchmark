using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Model;
using System.Text;

namespace DatabaseBenchmark.Databases.CosmosDb
{
    public class CosmosDbQueryBuilder : SqlQueryBuilder
    {
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

        protected override string BuildLimitClause()
        {
            var expression = new StringBuilder();

            if (Query.Skip > 0 || Query.Take > 0)
            {
                expression.AppendLine($"OFFSET {ParametersBuilder.Append(Query.Skip, ColumnType.Integer)}");
            }

            if (Query.Take > 0)
            {
                expression.AppendLine($"LIMIT {ParametersBuilder.Append(Query.Take, ColumnType.Integer)}");
            }

            return expression.ToString();
        }
    }
}
