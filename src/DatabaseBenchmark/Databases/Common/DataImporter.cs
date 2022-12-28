using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Model;
using System.Diagnostics;

namespace DatabaseBenchmark.Databases.Common
{
    public class DataImporter : IDataImporter
    {
        private readonly IQueryExecutor _insertExecutor;
        private readonly ITransactionProvider _transactionProvider;
        private readonly IDataMetricsProvider _dataMetricsProvider;
        private readonly IProgressReporter _progressReporter;

        public IDictionary<string, double> CustomMetrics { get; private set; }

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

            stopwatch.Stop();

            var result = new ImportResult(_dataMetricsProvider.GetRowCount(), stopwatch.ElapsedMilliseconds);
            if (CustomMetrics != null)
            {
                foreach (var metric in CustomMetrics)
                {
                    result.AddMetric(metric.Key, metric.Value);
                }
            }

            return result;
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

        public void Dispose()
        {
        }
    }
}
