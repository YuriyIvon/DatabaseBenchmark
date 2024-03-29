﻿using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.Databases.MongoDb
{
    [OptionPrefix("MongoDb")]
    public class MongoDbInsertOptions
    {
        [Option("Allows to collect request units metric in case the database is hosted by Azure CosmosDB")]
        public bool CollectCosmosDbRequestUnits { get; set; } = false;
    }
}
