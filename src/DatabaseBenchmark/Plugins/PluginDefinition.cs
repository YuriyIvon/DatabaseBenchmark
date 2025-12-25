using DatabaseBenchmark.Plugins.Interfaces;
using System.Text.Json;

namespace DatabaseBenchmark.Plugins
{
    public class PluginDefinition
    {
        public string Name { get; set; }

        public PluginType Type { get; set; }

        public JsonElement Options { get; set; }
    }
}
