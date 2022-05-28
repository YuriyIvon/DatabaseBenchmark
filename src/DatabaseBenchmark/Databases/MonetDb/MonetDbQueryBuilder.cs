using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Model;
using System.Text;

namespace DatabaseBenchmark.Databases.MonetDb
{
    public class MonetDbQueryBuilder : SqlQueryBuilder
    {
        public MonetDbQueryBuilder(
            Table table,
            Query query,
            SqlParametersBuilder parametersBuilder,
            IRandomValueProvider randomValueProvider) : base(table, query, parametersBuilder, randomValueProvider)
        {
        }

        protected override string BuildLimit()
        {
            var expression = new StringBuilder();

            if (Query.Take > 0)
            {
                expression.AppendLine($"LIMIT {ParametersBuilder.Append(Query.Take)}");
            }

            if (Query.Skip > 0)
            {
                expression.AppendLine($"OFFSET {ParametersBuilder.Append(Query.Skip)}");
            }

            return expression.ToString();
        }
    }
}
