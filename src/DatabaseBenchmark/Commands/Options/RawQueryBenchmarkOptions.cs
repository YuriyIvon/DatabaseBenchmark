using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core;
using DatabaseBenchmark.Databases;

namespace DatabaseBenchmark.Commands.Options
{
    public class RawQueryBenchmarkOptions : QueryExecutionOptions
    {
        [Option("Database type", true)]
        public string DatabaseType { get; set; }

        [Option("Connection string", true)]
        public string ConnectionString { get; set; }

        [Option("Table to be queried")]
        public string TableName { get; set; }

        [Option("Path to a text file with a raw query pattern to be executed", true)]
        public string QueryFilePath { get; set; }

        [Option("Path to a JSON file with a list of query parameter definitions")]
        public string QueryParametersFilePath { get; set; }
    }
}
