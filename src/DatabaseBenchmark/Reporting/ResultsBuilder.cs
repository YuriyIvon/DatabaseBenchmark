using DatabaseBenchmark.Reporting.Interfaces;
using DatabaseBenchmark.Utils;
using System.Data;

namespace DatabaseBenchmark.Reporting
{
    public class ResultsBuilder : IResultsBuilder
    {
        public DataTable Build(IEnumerable<MetricsCollection> metrics)
        {
            var results = new DataTable();

            var nameColumn = results.Columns.Add("name", typeof(string));
            nameColumn.Caption = "Name";

            var avgColumn = results.Columns.Add("avg", typeof(double));
            avgColumn.Caption = "Avg (ms)";

            var minColumn = results.Columns.Add("min", typeof(double));
            minColumn.Caption = "Min (ms)";

            var p10Column = results.Columns.Add("p10", typeof(double));
            p10Column.Caption = "10% (ms)";

            var p50Column = results.Columns.Add("p50", typeof(double));
            p50Column.Caption = "50% (ms)";

            var p90Column = results.Columns.Add("p90", typeof(double));
            p90Column.Caption = "90% (ms)";

            var maxColumn = results.Columns.Add("max", typeof(double));
            maxColumn.Caption = "Max (ms)";

            var qpsColumn = results.Columns.Add("qps", typeof(double));
            qpsColumn.Caption = "QPS";

            var avgRowsColumn = results.Columns.Add("avgRows", typeof(double));
            avgRowsColumn.Caption = "Avg Rows";

            foreach (var metric in metrics)
            {
                var row = results.Rows.Add();

                var durations = metric.Metrics
                    .Select(m => (m.EndTimestamp - m.StartTimestamp).TotalMilliseconds)
                    .ToArray();

                row[nameColumn] = metric.Name;
                row[avgColumn] = durations.Average();
                row[minColumn] = durations.Min();
                row[p10Column] = durations.Percentile(10);
                row[p50Column] = durations.Percentile(50);
                row[p90Column] = durations.Percentile(90);
                row[maxColumn] = durations.Max();
                row[qpsColumn] = GetThroughput(metric);
                row[avgRowsColumn] = metric.Metrics.Select(m => m.RowCount).Average();

                var customMetrics = metric.Metrics
                    .Where(m => m.CustomMetrics != null)
                    .Select(m => m.CustomMetrics);

                var customMetricTypes = customMetrics.SelectMany(cm => cm.Keys).Distinct();

                foreach (var metricType in customMetricTypes)
                {
                    var customAvgColumn = EnsureCustomMetricColumn(results, metricType, "avg", "Avg");
                    var customMinColumn = EnsureCustomMetricColumn(results, metricType, "min", "Min");
                    var customMaxColumn = EnsureCustomMetricColumn(results, metricType, "max", "Max");

                    row[customAvgColumn] = customMetrics.Average(cm => cm[metricType]);
                    row[customMinColumn] = customMetrics.Min(cm => cm[metricType]);
                    row[customMaxColumn] = customMetrics.Max(cm => cm[metricType]);
                }
            }

            return results;
        }

        private static double GetThroughput(MetricsCollection metrics)
        {
            var minTimestamp = metrics.Metrics.Min(m => m.StartTimestamp);
            var maxTimestamp = metrics.Metrics.Max(m => m.EndTimestamp);
            return metrics.Metrics.Count / (maxTimestamp - minTimestamp).TotalSeconds;
        }

        private static DataColumn EnsureCustomMetricColumn(DataTable dataTable, string metricType, string prefix, string title)
        {
            if (!dataTable.Columns.Contains(prefix + metricType))
            {
                var customMetricColumn = dataTable.Columns.Add(prefix + metricType, typeof(double));
                customMetricColumn.Caption = $"{title} ({metricType})";
                return customMetricColumn;
            }

            return dataTable.Columns[prefix + metricType];
        }
    }
}
