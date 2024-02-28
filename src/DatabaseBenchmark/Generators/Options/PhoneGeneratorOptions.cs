using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Generators.Options
{
    public class PhoneGeneratorOptions : GeneratorOptionsBase
    {
        public override GeneratorType Type => GeneratorType.Phone;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GeneratorKind Kind { get; set; } = GeneratorKind.PhoneNumber;

        public string Locale { get; set; }

        public enum GeneratorKind
        {
            PhoneNumber
        }
    }
}
