using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
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
            IDatabase database,
            Table table,
            IDataSource source,
            int batchSize,
            IExecutionEnvironment environment)
        {
            Container.RegisterInstance<Table>(table);
            Container.RegisterInstance<IDataSource>(source);
            Container.RegisterInstance<IExecutionEnvironment>(environment);

            var insertBuilderOptions = new InsertBuilderOptions { BatchSize = batchSize };
            Container.RegisterInstance<InsertBuilderOptions>(insertBuilderOptions);

            Container.RegisterSingleton<IDataSourceReader, DataSourceReader>();

            Container.Register<IDbConnection>(() => new TConnection { ConnectionString = database.ConnectionString }, Lifestyle);
            Container.Register<ISqlQueryBuilder, SqlInsertBuilder>(Lifestyle);
            Container.Register<ISqlParametersBuilder>(() => new SqlParametersBuilder(), Lifestyle);
            Container.Register<IQueryExecutor, SqlInsertExecutor>(Lifestyle);
        }
    }
}
