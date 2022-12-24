using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Model;
using System.Text;

namespace DatabaseBenchmark.Databases.ClickHouse
{
    public class ClickHouseQueryBuilder : SqlQueryBuilder
    {
        public ClickHouseQueryBuilder(
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
