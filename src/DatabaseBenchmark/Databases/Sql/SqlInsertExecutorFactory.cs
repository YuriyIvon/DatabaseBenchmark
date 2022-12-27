using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using System.Data;

namespace DatabaseBenchmark.Databases.Sql
{
    public class SqlInsertExecutorFactory<TConnection> : QueryExecutorFactoryBase
        where TConnection : class, IDbConnection, new()
    {
        public SqlInsertExecutorFactory(
            string connectionString,
            Table table,
            IDataSource source,
            IExecutionEnvironment environment)
        {
            Container.RegisterInstance<Table>(table);
            Container.RegisterInstance<IDataSource>(source);
            Container.RegisterInstance<IExecutionEnvironment>(environment);

            Container.RegisterSingleton<IDataSourceReader, DataSourceReader>();

            Container.Register<IDbConnection>(() => new TConnection { ConnectionString = connectionString }, Lifestyle);
            Container.Register<ISqlQueryBuilder, SqlInsertBuilder>(Lifestyle);
            Container.Register<ISqlParametersBuilder>(() => new SqlParametersBuilder(), Lifestyle);
            Container.Register<IQueryExecutor, SqlInsertExecutor>(Lifestyle);
        }
    }
}
