namespace DatabaseBenchmark.Reporting
{
    public class MetricsCollector
    {
        private readonly List<MetricsCollection> _collections = new();
        private MetricsCollection _currentMetrics;

        public IEnumerable<MetricsCollection> Collections => _collections;

        public void Start(string benchmarkName)
        {
            _currentMetrics = new MetricsCollection(benchmarkName);
            _collections.Add(_currentMetrics);
        }

        public void Abort()
        {
            _collections.Remove(_currentMetrics);
        }

        public void AppendResult(DateTime startTimestamp,
            DateTime endTimestamp,
            IDictionary<string, double> customMetrics)
        {
            _currentMetrics.Metrics.Add(new Metric(startTimestamp, endTimestamp, customMetrics));
        }
    }
}
