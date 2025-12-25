using Azure;
using Azure.AI.OpenAI;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Plugins.Interfaces;
using OpenAI.Embeddings;

namespace DatabaseBenchmark.Plugins.TextEmbedding
{
    public class AzureOpenAIEmbeddingModel : ITextEmbeddingModel
    {
        private readonly EmbeddingClient _client;

        public string Name { get; }

        public PluginType Type => PluginType.TextEmbeddingModel;

        public AzureOpenAIEmbeddingModel(string name, AzureOpenAIEmbeddingModelOptions options)
        {
            Name = name;
            ArgumentNullException.ThrowIfNull(options);

            if (string.IsNullOrEmpty(options.Endpoint))
            {
                throw new InputArgumentException("Endpoint is missing for the OpenAI model plugin");
            }

            if (string.IsNullOrEmpty(options.ApiKey))
            {
                throw new InputArgumentException("API key is missing for the OpenAI model plugin");
            }

            if (string.IsNullOrEmpty(options.DeploymentName))
            {
                throw new InputArgumentException("Deployment name is missing for the OpenAI model plugin");
            }

            var endpoint = new Uri(options.Endpoint);
            var credential = new AzureKeyCredential(options.ApiKey);

            AzureOpenAIClientOptions clientOptions = null;
            var openAIClient = new AzureOpenAIClient(endpoint, credential, clientOptions);
            _client = openAIClient.GetEmbeddingClient(options.DeploymentName);
        }

        public float[] GenerateEmbedding(string text, int? dimensions = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Text cannot be null or empty", nameof(text));
            }

            var embeddingOptions = new EmbeddingGenerationOptions();

            if (dimensions.HasValue && dimensions.Value > 0)
            {
                embeddingOptions.Dimensions = dimensions.Value;
            }

            var response = _client.GenerateEmbedding(text, embeddingOptions);

            return response.Value.ToFloats().ToArray();
        }
    }
}
