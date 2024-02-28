using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Generators.Options
{
    public class TextGeneratorOptions : GeneratorOptionsBase
    {
        public override GeneratorType Type => GeneratorType.Text;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GeneratorKind Kind { get; set; } = GeneratorKind.Sentence;

        public string Locale { get; set; }

        public enum GeneratorKind
        {
            Word,
            Sentence,
            Paragraph,
            Text
        }
    }
}
