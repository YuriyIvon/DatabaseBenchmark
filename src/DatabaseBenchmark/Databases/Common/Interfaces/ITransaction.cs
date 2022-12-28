namespace DatabaseBenchmark.Databases.Common.Interfaces
{
    public interface ITransaction : IDisposable
    {
        void Commit();

        void Rollback();
    }
}
