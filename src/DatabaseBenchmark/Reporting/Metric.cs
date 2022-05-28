namespace DatabaseBenchmark.Reporting
{
    public class Metric
    {
        public Metric(DateTime startTimestamp,
            DateTime endTimestamp,
            IDictionary<string, double> customMetrics = null)
        {
            StartTimestamp = startTimestamp;
            EndTimestamp = endTimestamp;
            CustomMetrics = customMetrics;
        }

        public DateTime StartTimestamp { get; }

        public DateTime EndTimestamp { get; }

        public IDictionary<string, double> CustomMetrics { get; }
    }
}
