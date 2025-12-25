namespace DatabaseBenchmark.Plugins.Interfaces
{
    public interface IPlugin
    {
        string Name { get; }

        PluginType Type { get; }
    }
}
