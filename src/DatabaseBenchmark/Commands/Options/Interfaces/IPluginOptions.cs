using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.Commands.Options.Interfaces
{
    public interface IPluginOptions
    {
        [Option("Path to the plugins definition file")]
        string PluginsFilePath { get; set; }
    }
}
