using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;
using SimpleInjector;
using System.Data;

namespace DatabaseBenchmark.Databases.Sql
{
    public class SqlRawQueryExecutorFactory<TConnection> : SqlQueryExecutorFactoryBase
        where TConnection : class, IDbConnection, new()
    {
        public SqlRawQueryExecutorFactory(
            string connectionString,
            RawQuery query,
            IExecutionEnvironment environment)
        {
            Container.Options.AllowOverridingRegistrations = true;

            Container.Register<IDbConnection>(() => new TConnection { ConnectionString = connectionString }, Lifestyle);
            Container.RegisterInstance<RawQuery>(query);
            Container.RegisterInstance<IExecutionEnvironment>(environment);
            Container.RegisterSingleton<IColumnPropertiesProvider, RawQueryColumnPropertiesProvider>();
            Container.RegisterSingleton<IRandomGenerator, RandomGenerator>();
            Container.Register<IDistinctValuesProvider, SqlDistinctValuesProvider>(Lifestyle);
            Container.Register<IRandomValueProvider, RandomValueProvider>(Lifestyle);
            Container.Register<SqlParametersBuilder>(() => new SqlParametersBuilder(), Lifestyle);
            Container.Register<ISqlQueryBuilder, SqlRawQueryBuilder>(Lifestyle);
            Container.Register<IQueryExecutor, SqlQueryExecutor>(Lifestyle);
        }
    }
}
