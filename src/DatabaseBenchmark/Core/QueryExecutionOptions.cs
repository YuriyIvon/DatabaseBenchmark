using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.Core
{
    public class QueryExecutionOptions
    {
        [Option("Name of the benchmark")]
        public string BenchmarkName { get; set; }

        [Option("Number of parallel threads")]
        public int QueryParallelism { get; set; } = 1;

        [Option("Number of executions on each thread")]
        public int QueryCount { get; set; } = 100;

        [Option("Number of warm-up executions on each thread")]
        public int WarmupQueryCount { get; set; } = 3;
    }
}
