using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Interfaces;

namespace DatabaseBenchmark.Databases.Sql
{
    public class SqlDataImporter
    {
        private readonly IQueryExecutor _insertExecutor;
        private readonly ITransactionProvider _transactionProvider;
        private readonly IProgressReporter _progressReporter;

        public SqlDataImporter(
            IQueryExecutor insertExecutor,
            ITransactionProvider transactionProvider,
            IProgressReporter progressReporter)
        {
            _insertExecutor = insertExecutor;
            _transactionProvider = transactionProvider;
            _progressReporter = progressReporter;
        }

        public void Import()
        {
            using var transaction = _transactionProvider.Begin();

            try
            {
                while (true)
                {
                    using var preparedInsert = _insertExecutor.Prepare(transaction);

                    if (preparedInsert == null)
                    {
                        break;
                    }

                    var rowsInserted = preparedInsert.Execute();
                    _progressReporter.Increment(rowsInserted);
                }

                transaction?.Commit();
            }
            catch
            {
                transaction?.Rollback();
                throw;
            }
        }
    }
}
