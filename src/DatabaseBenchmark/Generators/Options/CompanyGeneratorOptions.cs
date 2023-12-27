using DatabaseBenchmark.Generators.Interfaces;
using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Generators.Options
{
    public class CompanyGeneratorOptions : IGeneratorOptions
    {
        public GeneratorType Type => GeneratorType.Company;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GeneratorKind Kind { get; set; } = GeneratorKind.CompanyName;

        public enum GeneratorKind
        {
            CompanyName,
            CompanySuffix
        }
    }
}
