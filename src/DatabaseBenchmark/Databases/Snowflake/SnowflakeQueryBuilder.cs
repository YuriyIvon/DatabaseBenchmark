using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Model;
using System.Text;

namespace DatabaseBenchmark.Databases.Snowflake
{
    public class SnowflakeQueryBuilder : SqlQueryBuilder
    {
        public SnowflakeQueryBuilder(
            Table table,
            Query query,
            SqlQueryParametersBuilder parametersBuilder,
            IRandomValueProvider randomValueProvider,
            IRandomGenerator randomGenerator)
            : base(table, query, parametersBuilder, randomValueProvider, randomGenerator)
        {
        }

        protected override string BuildLimit()
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
