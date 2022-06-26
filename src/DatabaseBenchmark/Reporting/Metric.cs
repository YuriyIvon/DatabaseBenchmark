namespace DatabaseBenchmark.Reporting
{
    public class Metric
    {
        public Metric(
            DateTime startTimestamp,
            DateTime endTimestamp,
            int rowCount,
            IDictionary<string, double> customMetrics = null)
        {
            StartTimestamp = startTimestamp;
            EndTimestamp = endTimestamp;
            RowCount = rowCount;
            CustomMetrics = customMetrics;
        }

        public DateTime StartTimestamp { get; }

        public DateTime EndTimestamp { get; }

        public int RowCount { get; }

        public IDictionary<string, double> CustomMetrics { get; }
    }
}
