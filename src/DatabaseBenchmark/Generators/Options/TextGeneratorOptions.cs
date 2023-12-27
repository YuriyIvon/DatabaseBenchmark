using DatabaseBenchmark.Generators.Interfaces;
using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Generators.Options
{
    public class TextGeneratorOptions : IGeneratorOptions
    {
        public GeneratorType Type => GeneratorType.Text;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GeneratorKind Kind { get; set; } = GeneratorKind.Sentence;

        public enum GeneratorKind
        {
            Word,
            Sentence,
            Paragraph,
            Text
        }
    }
}
