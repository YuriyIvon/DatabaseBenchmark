using DatabaseBenchmark.Model;
using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Generators.Options
{
    public class EmbeddingGeneratorOptions : GeneratorOptionsBase
    {
        public override GeneratorType Type => GeneratorType.Embedding;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GeneratorKind Kind { get; set; } = GeneratorKind.Text;

        public string ModelName { get; set; }

        public int? Dimensions { get; set; }

        public IGeneratorOptions SourceGeneratorOptions { get; set; }

        public enum GeneratorKind
        {
            Text
        }
    }
}
