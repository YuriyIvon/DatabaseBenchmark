namespace DatabaseBenchmark.Databases.Common.Interfaces
{
    public interface IDataMetricsProvider
    {
        long GetRowCount();

        IDictionary<string, double> GetMetrics();
    }
}
