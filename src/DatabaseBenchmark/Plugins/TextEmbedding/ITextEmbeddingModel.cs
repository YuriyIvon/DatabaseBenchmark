using DatabaseBenchmark.Plugins.Interfaces;

namespace DatabaseBenchmark.Plugins.TextEmbedding
{
    public interface ITextEmbeddingModel : IPlugin
    {
        float[] GenerateEmbedding(string text, int? dimensions = null);
    }
}
