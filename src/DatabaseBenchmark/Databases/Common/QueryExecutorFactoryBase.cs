using DatabaseBenchmark.Databases.Common.Interfaces;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace DatabaseBenchmark.Databases.Common
{
    public class QueryExecutorFactoryBase : IQueryExecutorFactory
    {
        protected Container Container { get; } = new Container();

        protected Lifestyle Lifestyle { get; } = new AsyncScopedLifestyle(); 

        public QueryExecutorFactoryBase()
        {
            Container.Options.AllowOverridingRegistrations = true;
        }

        public IQueryExecutorFactory Customize<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class, TInterface
        {
            Container.Register<TInterface, TImplementation>(Lifestyle);

            return this;
        }

        public IQueryExecutorFactory Customize<TInterface>(Func<TInterface> factory)
            where TInterface : class
        {
            Container.Register(factory, Lifestyle);

            return this;
        }

        public IQueryExecutor Create()
        {
            var scope = AsyncScopedLifestyle.BeginScope(Container);
            return new ScopeHolder(Container.GetInstance<IQueryExecutor>(), scope);
        }

        private class ScopeHolder : IQueryExecutor
        {
            private readonly IQueryExecutor _executor;
            private readonly Scope _scope;

            public ScopeHolder(IQueryExecutor executor, Scope scope)
            {
                _executor = executor;
                _scope = scope;
            }

            public IPreparedQuery Prepare() => _executor.Prepare();

            public IPreparedQuery Prepare(ITransaction transaction) => _executor.Prepare(transaction);

            public void Dispose()
            {
                _executor.Dispose();
                _scope.Dispose();
            }
        }
    }
}
