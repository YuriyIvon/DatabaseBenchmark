namespace DatabaseBenchmark.Databases.Common.Interfaces
{
    public interface IQueryExecutorFactory
    {
        IQueryExecutor Create();
    }
}
