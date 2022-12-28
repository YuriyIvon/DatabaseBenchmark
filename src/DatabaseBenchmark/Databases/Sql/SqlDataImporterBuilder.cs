using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using SimpleInjector;
using System.Data;

namespace DatabaseBenchmark.Databases.Sql
{
    public class SqlDataImporterBuilder : DataImporterBuilder
    {
        public SqlDataImporterBuilder(Table table, IDataSource source, int batchSize, int defaultBatchSize)
            : base(table, source, batchSize, defaultBatchSize)
        {
            Container.Register<ISqlQueryBuilder, SqlInsertBuilder>(Lifestyle);
            Container.Register<IQueryExecutor, SqlInsertExecutor>(Lifestyle);
        }

        public SqlDataImporterBuilder Connection<T>(string connectionString)
            where T : IDbConnection, new()
        {
            Container.Register<IDbConnection>(() => new T { ConnectionString = connectionString }, Lifestyle);
            return this;
        }

        public SqlDataImporterBuilder ParametersBuilder<T>()
            where T : class, ISqlParametersBuilder
        {
            Container.Register<ISqlParametersBuilder, T>(Lifestyle);
            return this;
        }

        public SqlDataImporterBuilder ParametersBuilder<T>(Func<T> creator)
            where T : class, ISqlParametersBuilder
        {
            Container.Register<ISqlParametersBuilder>(creator, Lifestyle);
            return this;
        }

        public SqlDataImporterBuilder ParameterAdapter<T>()
            where T : class, ISqlParameterAdapter
        {
            Container.Register<ISqlParameterAdapter, T>(Lifestyle);
            return this;
        }
    }
}
