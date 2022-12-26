namespace DatabaseBenchmark.Databases.Interfaces
{
    public interface IPreparedQuery : IDisposable
    {
        IDictionary<string, double> CustomMetrics { get; }

        IQueryResults Results { get; }

        void Execute();
    }
}
