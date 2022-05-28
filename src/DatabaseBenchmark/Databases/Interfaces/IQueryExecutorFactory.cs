namespace DatabaseBenchmark.Databases.Interfaces
{
    public interface IQueryExecutorFactory
    {
        IQueryExecutor Create();
    }
}
