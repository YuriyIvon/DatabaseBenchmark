using DatabaseBenchmark.Commands.Options.Interfaces;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;

namespace DatabaseBenchmark.Commands.Options
{
    public class RawQueryBenchmarkOptions :
        ITargetOptions,
        IQueryExecutionOptions
    {
        public string DatabaseType { get; set; }

        public string ConnectionString { get; set; }

        public string TableName { get; set; }

        public string BenchmarkName { get; set; }

        public int QueryParallelism { get; set; } = 1;

        public int QueryCount { get; set; } = 100;

        public int WarmupQueryCount { get; set; } = 3;

        public int QueryDelay { get; set; } = 0;

        [Option("Path to a text file with a raw query pattern to be executed", true)]
        public string QueryFilePath { get; set; }

        [Option("Path to a JSON file with a list of query parameter definitions")]
        public string QueryParametersFilePath { get; set; }
    }
}
