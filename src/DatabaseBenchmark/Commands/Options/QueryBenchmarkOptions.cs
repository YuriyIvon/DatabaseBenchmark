using DatabaseBenchmark.Commands.Options.Interfaces;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;

namespace DatabaseBenchmark.Commands.Options
{
    public class QueryBenchmarkOptions :
        IStructuredTargetOptions,
        IQueryExecutionOptions
    {
        public string DatabaseType { get; set; }

        public string ConnectionString { get; set; }

        public string TableFilePath { get; set; }

        public string TableName { get; set; }

        public string BenchmarkName { get; set; }

        public int QueryParallelism { get; set; } = 1;

        public int QueryCount { get; set; } = 100;

        public int WarmupQueryCount { get; set; } = 3;

        [Option("Path to a JSON file describing the query to be executed", true)]
        public string QueryFilePath { get; set; }
    }
}
