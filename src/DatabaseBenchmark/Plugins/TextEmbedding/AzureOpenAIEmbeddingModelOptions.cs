namespace DatabaseBenchmark.Plugins.TextEmbedding
{
    public class AzureOpenAIEmbeddingModelOptions : TextEmbeddingModelOptions
    {
        public string ApiKey { get; set; }

        public string Endpoint { get; set; }

        public string DeploymentName { get; set; }
    }
}
