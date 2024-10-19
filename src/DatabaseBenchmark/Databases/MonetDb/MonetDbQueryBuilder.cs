using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;
using System.Text;

namespace DatabaseBenchmark.Databases.MonetDb
{
    public class MonetDbQueryBuilder : SqlQueryBuilder
    {
        public MonetDbQueryBuilder(
            Table table,
            Query query,
            ISqlParametersBuilder parametersBuilder,
            IRandomValueProvider randomValueProvider,
            IRandomPrimitives randomPrimitives) 
            : base(table, query, parametersBuilder, randomValueProvider, randomPrimitives)
        {
        }

        protected override string BuildLimitClause()
        {
            var expression = new StringBuilder();

            if (Query.Take > 0)
            {
                expression.AppendLine($"LIMIT {ParametersBuilder.Append(Query.Take, ColumnType.Integer, false)}");
            }

            if (Query.Skip > 0)
            {
                expression.AppendLine($"OFFSET {ParametersBuilder.Append(Query.Skip, ColumnType.Integer, false)}");
            }

            return expression.ToString();
        }
    }
}
