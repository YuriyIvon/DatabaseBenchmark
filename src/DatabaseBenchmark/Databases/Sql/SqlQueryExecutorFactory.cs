using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Model;
using SimpleInjector;
using System.Data;

namespace DatabaseBenchmark.Databases.Sql
{
    public class SqlQueryExecutorFactory<TConnection> : QueryExecutorFactoryBase
        where TConnection : class, IDbConnection, new()
    {
        public SqlQueryExecutorFactory(
            IDatabase database,
            Table table,
            Query query,
            IExecutionEnvironment environment)
        {
            Container.RegisterInstance<IDatabase>(database);
            Container.RegisterInstance<Table>(table);
            Container.RegisterInstance<Query>(query);
            Container.RegisterInstance<IExecutionEnvironment>(environment);
            Container.RegisterSingleton<IColumnPropertiesProvider, TableColumnPropertiesProvider>();
            Container.RegisterSingleton<IGeneratorFactory, DummyGeneratorFactory>();
            Container.RegisterSingleton<IRandomPrimitives, RandomPrimitives>();
            Container.RegisterSingleton<ICache, MemoryCache>();
            Container.RegisterDecorator<IDistinctValuesProvider, CachedDistinctValuesProvider>(Lifestyle);

            Container.Register<IDbConnection>(() => new TConnection { ConnectionString = database.ConnectionString }, Lifestyle);
            Container.Register<IDistinctValuesProvider, SqlDistinctValuesProvider>(Lifestyle);
            Container.Register<IRandomValueProvider, RandomValueProvider>(Lifestyle);
            Container.Register<ISqlParametersBuilder>(() => new SqlParametersBuilder(), Lifestyle);
            Container.Register<ISqlQueryBuilder, SqlQueryBuilder>(Lifestyle);
            Container.Register<IQueryExecutor, SqlQueryExecutor>(Lifestyle);
        }
    }
}
