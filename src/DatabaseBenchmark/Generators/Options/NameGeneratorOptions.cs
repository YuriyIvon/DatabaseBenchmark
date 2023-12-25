using DatabaseBenchmark.Generators.Interfaces;
using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Generators.Options
{
    public class NameGeneratorOptions : IGeneratorOptions
    {
        public GeneratorType Type => GeneratorType.Name;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GeneratorKind Kind { get; set; }

        public enum GeneratorKind
        {
            FirstName,
            LastName,
            FullName
        }
    }
}
