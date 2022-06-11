using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.DataSources.Database
{
    [OptionPrefix("DataSource.Database")]
    public class DatabaseDataSourceOptions
    {
        [Option("Database type", true)]
        public string DatabaseType { get; set; }

        [Option("Connection string", true)]
        public string ConnectionString { get; set; }

        [Option("Raw query returning the data", true)]
        public string QueryFilePath { get; set; }

        [Option("For NoSQL databases specifies a collection to be queried")]
        public string TableName { get; set; }
    }
}
