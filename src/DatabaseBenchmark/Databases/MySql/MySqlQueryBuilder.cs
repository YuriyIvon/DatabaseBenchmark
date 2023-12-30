using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Model;
using System.Text;

namespace DatabaseBenchmark.Databases.MySql
{
    public class MySqlQueryBuilder : SqlQueryBuilder
    {
        public MySqlQueryBuilder(
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

            if (Query.Take > 0 && Query.Skip > 0)
            {
                expression.AppendLine($"LIMIT {Query.Skip}, {Query.Take}");
            }
            else if (Query.Take > 0)
            {
                expression.AppendLine($"LIMIT {Query.Take}");
            }

            return expression.ToString();
        }
    }
}
