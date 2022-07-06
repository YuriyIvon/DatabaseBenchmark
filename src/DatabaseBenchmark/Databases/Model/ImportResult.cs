namespace DatabaseBenchmark.Databases.Model
{
    public class ImportResult
    {
        public long Count { get; }

        public long Duration { get; }

        public IDictionary<string, double> CustomMetrics { get; private set; }

        public ImportResult(long count, long duration)
        {
            Count = count;
            Duration = duration;
        }

        public void AddMetric(string name, double value)
        {
            if (CustomMetrics == null)
            {
                CustomMetrics = new Dictionary<string, double>();
            }

            CustomMetrics.Add(name, value);
        }
    }
}
