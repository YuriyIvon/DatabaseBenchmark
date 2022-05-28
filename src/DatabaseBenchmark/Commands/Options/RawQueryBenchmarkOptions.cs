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

        [Option("Path to a JSON file describing the query to be executed", true)]
        public string QueryFilePath { get; set; }
    }
}
