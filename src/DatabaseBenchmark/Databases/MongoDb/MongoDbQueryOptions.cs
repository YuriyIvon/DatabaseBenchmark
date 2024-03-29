﻿using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.Databases.MongoDb
{
    [OptionPrefix("MongoDb")]
    public class MongoDbQueryOptions
    {
        [Option("A maximum number of items to be fetched in one round-trip")]
        public int? BatchSize { get; set; }

        [Option("Allows to collect request units metric in case the database is hosted by Azure CosmosDB (may affect query timing)")]
        public bool CollectCosmosDbRequestUnits { get; set; } = false;
    }
}
