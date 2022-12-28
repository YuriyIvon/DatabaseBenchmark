using DatabaseBenchmark.Databases.Common.Interfaces;

namespace DatabaseBenchmark.Databases.Common
{
    public class NoTransactionProvider : ITransactionProvider
    {
        public ITransaction Begin() => null;
    }
}
