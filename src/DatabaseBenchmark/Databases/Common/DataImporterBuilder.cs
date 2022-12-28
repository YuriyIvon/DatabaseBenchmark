using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Model;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace DatabaseBenchmark.Databases.Common
{
    public class DataImporterBuilder
    {
        protected Container Container { get; } = new Container();

        protected Lifestyle Lifestyle { get; } = new AsyncScopedLifestyle();

        public DataImporterBuilder(Table table, IDataSource source, int batchSize, int defaultBatchSize)
        {
            Container.Options.AllowOverridingRegistrations = true;

            Container.RegisterInstance<Table>(table);
            Container.RegisterInstance<IDataSource>(source);

            var insertBuilderOptions = new InsertBuilderOptions
            {
                BatchSize = batchSize <= 0 ? defaultBatchSize : batchSize
            };
            Container.RegisterInstance<InsertBuilderOptions>(insertBuilderOptions);

            Container.Register<IDataSourceReader, DataSourceReader>(Lifestyle);
            Container.Register<IDataImporter, DataImporter>(Lifestyle);
        }

        public DataImporterBuilder OptionsProvider(IOptionsProvider optionsProvider)
        {
            Container.RegisterInstance<IOptionsProvider>(optionsProvider);
            return this;
        }

        public DataImporterBuilder Environment(IExecutionEnvironment environment)
        {
            Container.RegisterInstance<IExecutionEnvironment>(environment);
            return this;
        }

        //TODO: try to extract a common interface
        public DataImporterBuilder InsertBuilder<TService, TImplementation>()
            where TService : class 
            where TImplementation : class, TService
        {
            Container.Register<TService, TImplementation>(Lifestyle);
            return this;
        }

        public DataImporterBuilder InsertExecutor<T>()
            where T : class, IQueryExecutor
        {
            Container.Register<IQueryExecutor, T>(Lifestyle);
            return this;
        }

        public DataImporterBuilder TransactionProvider<T>()
            where T : class, ITransactionProvider
        {
            Container.Register<ITransactionProvider, T>(Lifestyle);
            return this;
        }

        public DataImporterBuilder DataMetricsProvider<T>()
            where T: class, IDataMetricsProvider
        {
            Container.Register<IDataMetricsProvider, T>(Lifestyle);
            return this;
        }

        public DataImporterBuilder DataMetricsProvider<T>(Action<T> initializer)
            where T : class, IDataMetricsProvider
        {
            DataMetricsProvider<T>();
            Container.RegisterInitializer<IDataMetricsProvider>(dmp => initializer((T)dmp));
            return this;
        }

        public DataImporterBuilder ProgressReporter<T>()
            where T: class, IProgressReporter
        {
            Container.Register<IProgressReporter, T>(Lifestyle);
            return this;
        }

        public DataImporterBuilder Customize(Action<Container, Lifestyle> customAction)
        {
            customAction(Container, Lifestyle);
            return this;
        }

        public IDataImporter Build()
        {
            var scope = AsyncScopedLifestyle.BeginScope(Container);
            return new ScopeHolder(Container.GetInstance<IDataImporter>(), scope);
        }

        private class ScopeHolder : IDataImporter
        {
            private readonly IDataImporter _importer;
            private readonly Scope _scope;

            public ScopeHolder(IDataImporter executor, Scope scope)
            {
                _importer = executor;
                _scope = scope;
            }

            public ImportResult Import() => _importer.Import();

            public void Dispose()
            {
                _importer.Dispose();
                _scope.Dispose();
            }
        }
    }
}
