using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System.Data;

namespace DatabaseBenchmark.Databases.Sql
{
    public class SqlQueryExecutorFactory<TConnection> : SqlQueryExecutorFactoryBase
        where TConnection : class, IDbConnection, new()
    {
        public SqlQueryExecutorFactory(
            string connectionString,
            Table table,
            Query query,
            IExecutionEnvironment environment)
        {
            Container.Register<IDbConnection>(() => new TConnection { ConnectionString = connectionString }, Lifestyle);
            Container.RegisterInstance<Table>(table);
            Container.RegisterInstance<Query>(query);
            Container.RegisterInstance<IExecutionEnvironment>(environment);
            Container.RegisterSingleton<IColumnPropertiesProvider, TableColumnPropertiesProvider>();
            Container.RegisterSingleton<IRandomGenerator, RandomGenerator>();
            Container.Register<IDistinctValuesProvider, SqlDistinctValuesProvider>(Lifestyle);
            Container.Register<IRandomValueProvider, RandomValueProvider>(Lifestyle);
            Container.Register<SqlParametersBuilder>(() => new SqlParametersBuilder(), Lifestyle);
            Container.Register<ISqlQueryBuilder, SqlQueryBuilder>(Lifestyle);
            Container.Register<IQueryExecutor, SqlQueryExecutor>(Lifestyle);
        }
    }
}
