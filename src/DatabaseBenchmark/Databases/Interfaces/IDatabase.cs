using DatabaseBenchmark.Databases.Model;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.Interfaces
{
    public interface IDatabase
    {
        void CreateTable(Table table, bool dropExisting);

        ImportResult ImportData(Table table, IDataSource source, int batchSize);

        IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query);

        IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query);

        IQueryExecutorFactory CreateInsertExecutorFactory(Table table, IDataSource dataSource);
    }
}
