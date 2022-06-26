using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Reporting;
using System.Data;

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

        public void Benchmark(IQueryExecutorFactory executorFactory, QueryExecutionOptions options)
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
                        BenchmarkInternal(executor, options.WarmupQueryCount, options.QueryCount);
                    },
                    TaskCreationOptions.LongRunning))
                .ToArray();

            Task.WaitAll(tasks);
        }

        private void BenchmarkInternal(
            IQueryExecutor executor,
            int warmupQueryCount,
            int queryCount)
        {
            for (int i = 0; i < warmupQueryCount; i++)
            {
                using var preparedQuery = executor.Prepare();
                preparedQuery.Execute();
                FetchResults(preparedQuery);
            }

            for (int i = 0; i < queryCount; i++)
            {
                using var preparedQuery = executor.Prepare();

                var startTimestamp = DateTime.Now;
                preparedQuery.Execute();
                var rowCount = FetchResults(preparedQuery);
                var endTimestamp = DateTime.Now;

                _metricsCollector.AppendResult(startTimestamp, endTimestamp, rowCount, preparedQuery.CustomMetrics);
            }
        }

        private int FetchResults(IPreparedQuery query)
        {
            var table = new DataTable();
            int rowCount = 0;

            while (query.Read())
            {
                rowCount++;

                if (_environment.TraceResults)
                {
                    foreach (var columnName in query.ColumnNames)
                    {
                        if (!table.Columns.Contains(columnName))
                        {
                            table.Columns.Add(columnName);
                        }
                    }

                    DataRow row = table.NewRow();

                    foreach (var columnName in query.ColumnNames)
                    {
                        row[columnName] = query.GetValue(columnName);
                    }

                    table.Rows.Add(row);
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
