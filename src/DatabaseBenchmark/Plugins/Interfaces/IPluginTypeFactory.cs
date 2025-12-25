using System.Text.Json;

namespace DatabaseBenchmark.Plugins.Interfaces
{
    public interface IPluginTypeFactory
    {
        PluginType Type { get; }

        IPlugin Create(string name, JsonElement optionsJson);
    }
}
