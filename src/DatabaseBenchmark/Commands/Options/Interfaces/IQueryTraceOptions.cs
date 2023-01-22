using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.Commands.Options.Interfaces
{
    public interface IQueryTraceOptions
    {
        [Option("Trace queries text and parameters")]
        bool TraceQueries { get; set; }
    }
}
