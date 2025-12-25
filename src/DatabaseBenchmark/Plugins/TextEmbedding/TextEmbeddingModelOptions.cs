using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Plugins.TextEmbedding
{
    public class TextEmbeddingModelOptions
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ModelKind Kind { get; set; } = ModelKind.AzureOpenAI;

        public enum ModelKind
        {
            AzureOpenAI
        }
    }
}
