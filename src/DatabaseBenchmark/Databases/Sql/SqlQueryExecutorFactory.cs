using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;
using SimpleInjector;
using System.Data;

namespace DatabaseBenchmark.Databases.Sql
{
    public class SqlQueryExecutorFactory<TConnection> : QueryExecutorFactoryBase
        where TConnection : class, IDbConnection, new()
    {
        public SqlQueryExecutorFactory(
            string connectionString,
            Table table,
            Query query,
            IExecutionEnvironment environment)
        {
            Container.RegisterInstance<Table>(table);
            Container.RegisterInstance<Query>(query);
            Container.RegisterInstance<IExecutionEnvironment>(environment);
            Container.RegisterSingleton<IColumnPropertiesProvider, TableColumnPropertiesProvider>();
            Container.RegisterSingleton<IRandomGenerator, RandomGenerator>();
            Container.RegisterSingleton<ICache, MemoryCache>();
            Container.RegisterDecorator<IDistinctValuesProvider, CachedDistinctValuesProvider>(Lifestyle);

            Container.Register<IDbConnection>(() => new TConnection { ConnectionString = connectionString }, Lifestyle);
            Container.Register<IDistinctValuesProvider, SqlDistinctValuesProvider>(Lifestyle);
            Container.Register<IRandomValueProvider, RandomValueProvider>(Lifestyle);
            Container.Register<SqlQueryParametersBuilder>(() => new SqlQueryParametersBuilder(), Lifestyle);
            Container.Register<ISqlQueryBuilder, SqlQueryBuilder>(Lifestyle);
            Container.Register<IQueryExecutor, SqlQueryExecutor>(Lifestyle);
        }
    }
}
