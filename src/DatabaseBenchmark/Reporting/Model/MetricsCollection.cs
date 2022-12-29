using System.Collections.Concurrent;

namespace DatabaseBenchmark.Reporting.Model
{
    public class MetricsCollection
    {
        public MetricsCollection(string name) => Name = name;

        public string Name { get; }

        public ConcurrentBag<Metric> Metrics { get; } = new();
    }
}
