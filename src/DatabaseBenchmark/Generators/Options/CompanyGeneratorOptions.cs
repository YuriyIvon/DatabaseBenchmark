using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Generators.Options
{
    public class CompanyGeneratorOptions : GeneratorOptionsBase
    {
        public override GeneratorType Type => GeneratorType.Company;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GeneratorKind Kind { get; set; } = GeneratorKind.CompanyName;

        public string Locale { get; set; }

        public enum GeneratorKind
        {
            CompanyName,
            CompanySuffix
        }
    }
}
