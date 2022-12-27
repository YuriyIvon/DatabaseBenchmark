using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Interfaces;

namespace DatabaseBenchmark.Databases.Common
{
    public class DataImporter
    {
        private readonly IQueryExecutor _insertExecutor;
        private readonly ITransactionProvider _transactionProvider;
        private readonly IProgressReporter _progressReporter;

        public IDictionary<string, double> CustomMetrics { get; private set; }

        public DataImporter(
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

                    CollectMetrics(preparedInsert);
                }

                transaction?.Commit();
            }
            catch
            {
                transaction?.Rollback();
                throw;
            }
        }

        private void CollectMetrics(IPreparedQuery preparedInsert)
        {
            if (preparedInsert.CustomMetrics != null)
            {
                CustomMetrics ??= new Dictionary<string, double>();

                foreach (var metric in preparedInsert.CustomMetrics)
                {
                    if (!CustomMetrics.ContainsKey(metric.Key))
                    {
                        CustomMetrics.Add(metric.Key, metric.Value);
                    }
                    else
                    {
                        CustomMetrics[metric.Key] += metric.Value;
                    }
                }
            }
        }
    }
}
