namespace DatabaseBenchmark.Databases.Interfaces
{
    public interface IPreparedQuery : IDisposable
    {
        IEnumerable<string> ColumnNames { get; }

        IDictionary<string, double> CustomMetrics { get; }

        void Execute();

        object GetValue(string columnName);

        bool Read();
    }
}
