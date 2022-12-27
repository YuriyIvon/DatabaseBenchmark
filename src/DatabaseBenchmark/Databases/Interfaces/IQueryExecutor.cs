namespace DatabaseBenchmark.Databases.Interfaces
{
    public interface IQueryExecutor : IDisposable
    {
        public IPreparedQuery Prepare();

        public IPreparedQuery Prepare(ITransaction transaction);
    }
}
