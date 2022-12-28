namespace DatabaseBenchmark.Databases.Common.Interfaces
{
    public interface ITransactionProvider
    {
        ITransaction Begin();
    }
}
