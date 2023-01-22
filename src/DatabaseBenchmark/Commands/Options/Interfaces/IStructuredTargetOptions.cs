using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.Commands.Options.Interfaces
{
    interface IStructuredTargetOptions : ITargetOptions
    {
        [Option("Path to a JSON file describing the table structure", true)]
        string TableFilePath { get; set; }
    }
}
