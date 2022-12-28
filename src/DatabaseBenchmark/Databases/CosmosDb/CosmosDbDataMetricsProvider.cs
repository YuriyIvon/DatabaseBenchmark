using DatabaseBenchmark.Databases.Common.Interfaces;
using Microsoft.Azure.Cosmos;

namespace DatabaseBenchmark.Databases.CosmosDb
{
    public class CosmosDbDataMetricsProvider : IDataMetricsProvider
    {
        private readonly Container _container;

        public CosmosDbDataMetricsProvider(Container container)
        {
            _container = container;
        }

        public long GetRowCount() => _container.Count();
    }
}
