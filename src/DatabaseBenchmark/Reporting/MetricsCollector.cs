using System.Collections.Concurrent;

namespace DatabaseBenchmark.Reporting
{
    public class MetricsCollector
    {
        private readonly ConcurrentBag<MetricsCollection> _collections = new();
        private MetricsCollection _currentMetrics;

        public IEnumerable<MetricsCollection> Collections => _collections;

        public void Start(string benchmarkName)
        {
            _currentMetrics = new MetricsCollection(benchmarkName);
            _collections.Add(_currentMetrics);
        }

        public void Abort()
        {
            _collections.TryTake(out var _);
        }

        public void AppendResult(DateTime startTimestamp,
            DateTime endTimestamp,
            IDictionary<string, double> customMetrics)
        {
            _currentMetrics.Metrics.Add(new Metric(startTimestamp, endTimestamp, customMetrics));
        }
    }
}
