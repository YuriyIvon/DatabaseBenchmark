namespace DatabaseBenchmark.Databases.Common.Interfaces
{
    public interface IPreparedQuery : IDisposable
    {
        IDictionary<string, double> CustomMetrics { get; }

        IQueryResults Results { get; }

        int Execute();
    }
}
