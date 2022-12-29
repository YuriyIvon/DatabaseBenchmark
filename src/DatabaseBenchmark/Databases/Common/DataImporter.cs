using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Model;
using System.Diagnostics;

namespace DatabaseBenchmark.Databases.Common
{
    public sealed class DataImporter : IDataImporter
    {
        private readonly IQueryExecutor _insertExecutor;
        private readonly ITransactionProvider _transactionProvider;
        private readonly IDataMetricsProvider _dataMetricsProvider;
        private readonly IProgressReporter _progressReporter;

        public DataImporter(
            IQueryExecutor insertExecutor,
            ITransactionProvider transactionProvider,
            IDataMetricsProvider dataMetricsProvider,
            IProgressReporter progressReporter)
        {
            _insertExecutor = insertExecutor;
            _transactionProvider = transactionProvider;
            _dataMetricsProvider = dataMetricsProvider;
            _progressReporter = progressReporter;
        }

        public ImportResult Import()
        {
            var stopwatch = Stopwatch.StartNew();
            var metrics = new Dictionary<string, double>();

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

                    CollectMetrics(preparedInsert.CustomMetrics, metrics);
                }

                transaction?.Commit();
            }
            catch
            {
                transaction?.Rollback();
                throw;
            }

            stopwatch.Stop();

            var result = new ImportResult(_dataMetricsProvider.GetRowCount(), stopwatch.ElapsedMilliseconds);
            CollectMetrics(_dataMetricsProvider.GetMetrics(), metrics);
            result.AddMetrics(metrics);

            return result;
        }

        private static void CollectMetrics(IDictionary<string, double> from, IDictionary<string, double> to)
        {
            if (from != null)
            {
                foreach (var metric in from)
                {
                    if (!to.ContainsKey(metric.Key))
                    {
                        to.Add(metric.Key, metric.Value);
                    }
                    else
                    {
                        to[metric.Key] += metric.Value;
                    }
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
