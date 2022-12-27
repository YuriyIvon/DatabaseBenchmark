namespace DatabaseBenchmark.Databases.Interfaces
{
    public interface ITransactionProvider
    {
        ITransaction Begin();
    }
}
