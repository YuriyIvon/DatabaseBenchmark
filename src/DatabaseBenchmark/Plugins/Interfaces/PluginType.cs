using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Plugins.Interfaces
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PluginType
    {
        TextEmbeddingModel
    }
}
