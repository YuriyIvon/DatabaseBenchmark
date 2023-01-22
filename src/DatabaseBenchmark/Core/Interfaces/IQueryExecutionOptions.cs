using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.Core.Interfaces
{
    public interface IQueryExecutionOptions
    {
        [Option("Name of the benchmark")]
        string BenchmarkName { get; set; }

        [Option("Number of parallel threads")]
        int QueryParallelism { get; set; }

        [Option("Number of executions on each thread")]
        int QueryCount { get; set; }

        [Option("Number of warm-up executions on each thread")]
        int WarmupQueryCount { get; set; }

        [Option("Delay in milliseconds between query executions")]
        int QueryDelay { get; set; }
    }
}
