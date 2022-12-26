using DatabaseBenchmark.Databases.Interfaces;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBenchmark.Databases.CosmosDb
{
    public class CosmosDbPreparedInsert : IPreparedQuery
    {
        private readonly Container _container;

        private double _requestCharge;

        public IDictionary<string, double> CustomMetrics =>
            new Dictionary<string, double> { [CosmosDbConstants.RequestUnitsMetric] = _requestCharge };

        public IQueryResults Results => null;

        public CosmosDbPreparedInsert(Container container)
        {
            _container = container;
        }

        public void Execute()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
