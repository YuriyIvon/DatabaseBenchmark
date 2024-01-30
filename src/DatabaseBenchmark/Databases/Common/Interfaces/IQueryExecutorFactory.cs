namespace DatabaseBenchmark.Databases.Common.Interfaces
{
    public interface IQueryExecutorFactory
    {
        IQueryExecutor Create();

        IQueryExecutorFactory Customize<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class, TInterface;

        IQueryExecutorFactory Customize<TInterface>(Func<TInterface> factory)
            where TInterface : class;
    }
}
