using System.Collections.Concurrent;

namespace DatabaseBenchmark.Reporting
{
    public class MetricsCollection
    {
        public MetricsCollection(string name) => Name = name;

        public string Name { get; }

        public ConcurrentBag<Metric> Metrics { get; } = new();
    }
}
