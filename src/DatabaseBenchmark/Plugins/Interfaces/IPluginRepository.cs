namespace DatabaseBenchmark.Plugins.Interfaces
{
    public interface IPluginRepository
    {
        T GetPlugin<T>(string name, PluginType type) where T : class, IPlugin;

        bool TryGetPlugin<T>(string name, PluginType type, out T plugin) where T : class, IPlugin;
    }
}
