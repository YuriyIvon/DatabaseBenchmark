namespace DatabaseBenchmark.Databases.Common.Interfaces
{
    public interface IQueryExecutor : IDisposable
    {
        public IPreparedQuery Prepare();

        public IPreparedQuery Prepare(ITransaction transaction);
    }
}
