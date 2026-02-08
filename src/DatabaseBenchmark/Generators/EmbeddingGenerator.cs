using DatabaseBenchmark.Common;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using DatabaseBenchmark.Plugins.Interfaces;
using DatabaseBenchmark.Plugins.TextEmbedding;
using System.Collections.Concurrent;

namespace DatabaseBenchmark.Generators
{
    public class EmbeddingGenerator : IGenerator
    {
        private readonly IGenerator _sourceGenerator;
        private readonly ITextEmbeddingModel _embeddingModel;
        private readonly int? _dimensions;
        private readonly bool _cache;
        private readonly ConcurrentDictionary<string, float[]> _embeddingsCache;

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
            _cache = options.Cache;

            if (_cache)
            {
                _embeddingsCache = new ConcurrentDictionary<string, float[]>();
            }
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
                Current = GetOrComputeEmbedding(text);
            }
            else
            {
                Current = null;
            }

            return true;
        }

        private float[] GetOrComputeEmbedding(string text) =>
            _cache
                ? _embeddingsCache.GetOrAdd(text, t => _embeddingModel.GenerateEmbedding(t, _dimensions))
                : _embeddingModel.GenerateEmbedding(text, _dimensions);
    }
}
