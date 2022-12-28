using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.Common.Interfaces
{
    public interface IDatabase
    {
        void CreateTable(Table table, bool dropExisting);

        IDataImporter CreateDataImporter(Table table, IDataSource source, int batchSize);

        IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query);

        IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query);

        IQueryExecutorFactory CreateInsertExecutorFactory(Table table, IDataSource dataSource, int batchSize);
    }
}
