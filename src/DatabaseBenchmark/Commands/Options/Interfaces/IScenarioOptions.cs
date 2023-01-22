using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.Commands.Options.Interfaces
{
    public interface IScenarioOptions
    {
        [Option("Path to a JSON file describing the query scenario", true)]
        string QueryScenarioFilePath { get; set; }

        [Option("Comma-separated list of scenario item indexes to run")]
        string QueryScenarioStepIndexes { get; set; }

        [Option("Path to JSON file specifying scenario parameters")]
        string QueryScenarioParametersFilePath { get; set; }
    }
}
