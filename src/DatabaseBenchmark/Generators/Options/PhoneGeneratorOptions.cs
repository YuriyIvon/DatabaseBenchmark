using DatabaseBenchmark.Generators.Interfaces;
using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Generators.Options
{
    public class PhoneGeneratorOptions : IGeneratorOptions
    {
        public GeneratorType Type => GeneratorType.Phone;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GeneratorKind Kind { get; set; } = GeneratorKind.PhoneNumber;

        public enum GeneratorKind
        {
            PhoneNumber
        }
    }
}
