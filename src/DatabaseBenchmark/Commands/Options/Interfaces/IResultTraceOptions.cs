using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.Commands.Options.Interfaces
{
    public interface IResultTraceOptions
    {
        [Option("Trace query results")]
        bool TraceResults { get; set; }
    }
}
