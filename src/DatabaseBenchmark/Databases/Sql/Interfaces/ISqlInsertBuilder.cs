namespace DatabaseBenchmark.Databases.Sql.Interfaces
{
    public interface ISqlInsertBuilder : ISqlQueryBuilder
    {
        int BatchSize { get; }
    }
}
