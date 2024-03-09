using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Generators.Options
{
    public class PatternGeneratorOptions : GeneratorOptionsBase
    {
        public override GeneratorType Type => GeneratorType.Pattern;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GeneratorKind Kind { get; set; } = GeneratorKind.Simple;

        public string Pattern { get; set; }

        public enum GeneratorKind
        {
            Simple,
            Regex
        }
    }
}
