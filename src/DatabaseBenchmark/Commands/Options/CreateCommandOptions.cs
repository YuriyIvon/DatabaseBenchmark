using DatabaseBenchmark.Common;
using DatabaseBenchmark.Databases;

namespace DatabaseBenchmark.Commands.Options
{
    public class CreateCommandOptions
    {
        [Option("Database type", true)]
        public string DatabaseType { get; set; }

        [Option("Connection string", true)]
        public string ConnectionString { get; set; }

        [Option("Path to a JSON file describing the table structure", true)]
        public string TableFilePath { get; set; }

        [Option("Target physical table name")]
        public string TableName { get; set; }

        [Option("Trace queries text and parameters")]
        public bool TraceQueries { get; set; } = false;
    }
}
