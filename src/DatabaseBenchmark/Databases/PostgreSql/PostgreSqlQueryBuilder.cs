using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;
using System.Text.Json;

namespace DatabaseBenchmark.Databases.PostgreSql
{
    public class PostgreSqlQueryBuilder : SqlQueryBuilder
    {
        protected static readonly QueryPrimitiveOperator[] AllowedArrayOperators =
        [
            QueryPrimitiveOperator.Equals,
            QueryPrimitiveOperator.NotEquals,
            QueryPrimitiveOperator.Contains
        ];

        private readonly PostgreSqlQueryOptions _queryOptions;

        public PostgreSqlQueryBuilder(
            Table table,
            Query query,
            ISqlParametersBuilder parametersBuilder,
            IRandomValueProvider randomValueProvider,
            IRandomPrimitives randomPrimitives,
            IOptionsProvider optionsProvider)
            : base(table, query, parametersBuilder, randomValueProvider, randomPrimitives)
        {
            _queryOptions = optionsProvider.GetOptions<PostgreSqlQueryOptions>();
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
                return _queryOptions.UseArrayGinOperators
                    ? $"{columnReference} @> '{{{FormatValue(value)}}}'"
                    : $"{ParametersBuilder.Append(value, column.Type, false)} = ANY({columnReference})";
            }

            return base.BuildAdvancedOperatorCondition(condition, value);
        }

        protected static string FormatValue(object value) =>
            JsonSerializer.Serialize(value).Replace("'", "''");
    }
}
