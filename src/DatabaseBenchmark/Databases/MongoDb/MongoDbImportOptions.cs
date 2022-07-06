using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.Databases.MongoDb
{
    [OptionPrefix("MongoDb")]
    public class MongoDbImportOptions
    {
        [Option("Allows to collect request units metric in case the database is hosted by Azure CosmosDB (may affect query timing)")]
        public bool CollectCosmosDbRequestUnits { get; set; } = false;
    }
}
