namespace DatabaseBenchmark.DataSources.Interfaces
{
    public interface IMaxRowsAwareDataSource
    {
        void SetMaxRows(int maxRows);
    }
}
