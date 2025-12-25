using DatabaseBenchmark.Common;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using DatabaseBenchmark.Plugins.Interfaces;
using DatabaseBenchmark.Plugins.TextEmbedding;

namespace DatabaseBenchmark.Generators
{
    public class EmbeddingGenerator : IGenerator
    {
        private readonly IGenerator _sourceGenerator;
        private readonly ITextEmbeddingModel _embeddingModel;
        private readonly int? _dimensions;

        public object Current { get; private set; }

        public bool IsBounded => _sourceGenerator.IsBounded;

        public EmbeddingGenerator(EmbeddingGeneratorOptions options, IGenerator sourceGenerator, IPluginRepository pluginRepository)
        {
            ArgumentNullException.ThrowIfNull(options);
            _sourceGenerator = sourceGenerator ?? throw new ArgumentNullException(nameof(sourceGenerator));
            ArgumentNullException.ThrowIfNull(pluginRepository);

            if (string.IsNullOrEmpty(options.ModelName))
            {
                throw new InputArgumentException("Model name is required for EmbeddingGenerator");
            }

            //TODO: refactor resolution and interfaces when non-text embedding models are added
            _embeddingModel = pluginRepository.GetPlugin<ITextEmbeddingModel>(options.ModelName, PluginType.TextEmbeddingModel);
            _dimensions = options.Dimensions;
        }

        public bool Next()
        {
            if (!_sourceGenerator.Next())
            {
                return false;
            }

            var sourceValue = _sourceGenerator.Current;

            if (sourceValue != null)
            {
                //TODO: refactor when non-text embedding models are added
                var text = (string)sourceValue;
                var embedding = _embeddingModel.GenerateEmbedding(text, _dimensions);
                Current = embedding;
            }
            else
            {
                Current = null;
            }

            return true;
        }
    }
}
