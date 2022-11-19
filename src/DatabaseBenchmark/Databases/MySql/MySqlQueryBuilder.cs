using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Model;
using System.Text;

namespace DatabaseBenchmark.Databases.MySql
{
    public class MySqlQueryBuilder : SqlQueryBuilder
    {
        public MySqlQueryBuilder(
            Table table,
            Query query,
            SqlParametersBuilder parametersBuilder,
            IRandomValueProvider randomValueProvider,
            IRandomGenerator randomGenerator) 
            : base(table, query, parametersBuilder, randomValueProvider, randomGenerator)
        {
        }

        protected override string BuildLimit()
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
