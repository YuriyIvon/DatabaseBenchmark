using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using System.Data;

namespace DatabaseBenchmark.Databases.Sql
{
    public sealed class SqlInsertExecutor : SqlExecutorBase
    {
        public SqlInsertExecutor(
            IDbConnection connection,
            ISqlQueryBuilder queryBuilder,
            ISqlParametersBuilder parametersBuilder,
            ISqlParameterAdapter parameterAdapter,
            IExecutionEnvironment environment)
            : base(connection, queryBuilder, parametersBuilder, parameterAdapter, environment)
        {
        }

        protected override IPreparedQuery CreatePreparedStatement(IDbCommand command) =>
            new SqlPreparedInsert(command);
    }
}
