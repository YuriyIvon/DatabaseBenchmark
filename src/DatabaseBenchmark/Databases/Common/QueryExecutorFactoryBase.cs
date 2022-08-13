using DatabaseBenchmark.Databases.Interfaces;
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

        public QueryExecutorFactoryBase Customize<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class, TInterface
        {
            Container.Register<TInterface, TImplementation>();

            return this;
        }

        public QueryExecutorFactoryBase Customize<TInterface>(Func<TInterface> factory)
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

            public void Dispose()
            {
                _executor.Dispose();
                _scope.Dispose();
            }
        }
    }
}
