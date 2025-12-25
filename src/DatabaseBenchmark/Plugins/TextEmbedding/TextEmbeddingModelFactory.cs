using DatabaseBenchmark.Common;
using DatabaseBenchmark.Plugins.Interfaces;
using System.Text.Json;

namespace DatabaseBenchmark.Plugins.TextEmbedding
{
    public class TextEmbeddingModelFactory : IPluginTypeFactory
    {
        public PluginType Type => PluginType.TextEmbeddingModel;

        public IPlugin Create(string name, JsonElement optionsJson)
        {
            if (!optionsJson.TryGetProperty(nameof(TextEmbeddingModelOptions.Kind), out var kindElement))
            {
                throw new InputArgumentException("Property \"Kind\" not found in text embedding model options");
            }

            var kind = Enum.Parse<TextEmbeddingModelOptions.ModelKind>(kindElement.GetString());

            return kind switch
            {
                TextEmbeddingModelOptions.ModelKind.AzureOpenAI => CreateAzureOpenAI(name, optionsJson),
                _ => throw new InputArgumentException($"Unknown text embedding model kind \"{kind}\"")
            };
        }

        private IPlugin CreateAzureOpenAI(string name, JsonElement optionsJson)
        {
            var options = JsonSerializer.Deserialize<AzureOpenAIEmbeddingModelOptions>(optionsJson);

            return new AzureOpenAIEmbeddingModel(name, options);
        }
    }
}
