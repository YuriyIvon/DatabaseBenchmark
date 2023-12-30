using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Generators.Options
{
    public class NameGeneratorOptions : GeneratorOptionsBase
    {
        public override GeneratorType Type => GeneratorType.Name;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GeneratorKind Kind { get; set; } = GeneratorKind.FirstName;

        public enum GeneratorKind
        {
            FirstName,
            LastName,
            FullName
        }
    }
}
