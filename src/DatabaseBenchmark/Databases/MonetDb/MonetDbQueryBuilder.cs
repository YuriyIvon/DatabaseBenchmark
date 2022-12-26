using DatabaseBenchmark.Core.Interfaces;
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
            IRandomGenerator randomGenerator) 
            : base(table, query, parametersBuilder, randomValueProvider, randomGenerator)
        {
        }

        protected override string BuildLimit()
        {
            var expression = new StringBuilder();

            if (Query.Take > 0)
            {
                expression.AppendLine($"LIMIT {ParametersBuilder.Append(Query.Take, ColumnType.Integer)}");
            }

            if (Query.Skip > 0)
            {
                expression.AppendLine($"OFFSET {ParametersBuilder.Append(Query.Skip, ColumnType.Integer)}");
            }

            return expression.ToString();
        }
    }
}
