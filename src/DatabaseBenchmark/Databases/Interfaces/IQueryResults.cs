namespace DatabaseBenchmark.Databases.Interfaces
{
    public interface IQueryResults
    {
        IEnumerable<string> ColumnNames { get; }

        object GetValue(string columnName);

        bool Read();
    }
}
