using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Reporting;

namespace DatabaseBenchmark.Core
{
    public class QueryBenchmark
    {
        private readonly MetricsCollector _metricsCollector;
        private readonly IExecutionEnvironment _environment;

        public QueryBenchmark(IExecutionEnvironment environment, MetricsCollector metricsCollector)
        {
            _environment = environment;
            _metricsCollector = metricsCollector;
        }

        public void Benchmark(IQueryExecutorFactory executorFactory, IQueryExecutionOptions options)
        {
            _environment.Write("Running query benchmark");

            if (!string.IsNullOrEmpty(options.BenchmarkName))
            {
                _environment.Write($" \"{options.BenchmarkName}\"");
            }

            _environment.WriteLine(string.Empty);

            _metricsCollector.Start(options.BenchmarkName);

            var tasks = Enumerable.Range(0, options.QueryParallelism)
                .Select(_ =>
                    Task.Factory.StartNew(() =>
                    {
                        using var executor = executorFactory.Create();
                        BenchmarkInternal(executor, options);
                    },
                    TaskCreationOptions.LongRunning))
                .ToArray();

            Task.WaitAll(tasks);
        }

        private void BenchmarkInternal(
            IQueryExecutor executor,
            IQueryExecutionOptions options)
        {
            try
            {
                BenchmarkQuery(executor, options.WarmupQueryCount, options.QueryDelay, null);
                BenchmarkQuery(executor, options.QueryCount, options.QueryDelay, _metricsCollector);
            }
            catch (NoDataAvailableException)
            {
                _environment.WriteLine("WARNING: No more queries left");
            }
        }

        private void BenchmarkQuery(
            IQueryExecutor executor,
            int count,
            int delay,
            MetricsCollector metricsCollector)
        {
            for (int i = 0; i < count; i++)
            {
                using var preparedQuery = executor.Prepare();

                var startTimestamp = DateTime.UtcNow;
                var rowCount = ExecuteQuery(preparedQuery);
                var endTimestamp = DateTime.UtcNow;

                metricsCollector?.AppendResult(startTimestamp, endTimestamp, rowCount, preparedQuery.CustomMetrics);

                if (delay > 0)
                {
                    Thread.Sleep(delay);
                }
            }
        }

        private int ExecuteQuery(IPreparedQuery query)
        {
            var rowCount = query.Execute();

            if (query.Results != null)
            {
                rowCount = FetchResults(query);
            }

            return rowCount;
        }

        private int FetchResults(IPreparedQuery query)
        {
            var results = query.Results;

            var table = new LightweightDataTable();
            int rowCount = 0;

            while (results.Read())
            {
                rowCount++;

                if (_environment.TraceResults)
                {
                    foreach (var columnName in results.ColumnNames)
                    {
                        if (!table.HasColumn(columnName))
                        {
                            table.AddColumn(columnName);
                        }
                    }

                    LightweightDataRow row = table.AddRow();

                    foreach (var columnName in results.ColumnNames)
                    {
                        row[columnName] = results.GetValue(columnName);
                    }
                }
            }

            if (_environment.TraceResults)
            {
                _environment.WriteTable(table);
            }

            return rowCount;
        }
    }
}
