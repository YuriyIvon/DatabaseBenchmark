using DatabaseBenchmark.Databases.CosmosDb.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;

namespace DatabaseBenchmark.Databases.CosmosDb
{
    public class CosmosDbInsertBuilder : ICosmosDbInsertBuilder
    {
        private readonly IDataSource _source;

        public int BatchSize { get; set; } = 1;

        public CosmosDbInsertBuilder(IDataSource source)
        {
            _source = source;
        }

        public IEnumerable<object> Build()
        {
            throw new NotImplementedException();
        }
    }
}
