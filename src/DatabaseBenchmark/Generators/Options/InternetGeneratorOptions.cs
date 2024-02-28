using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Generators.Options
{
    public class InternetGeneratorOptions : GeneratorOptionsBase
    {
        public override GeneratorType Type => GeneratorType.Internet;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GeneratorKind Kind { get; set; } = GeneratorKind.Email;

        public string Locale { get; set; }

        public enum GeneratorKind
        {
            DomainName,
            Email,
            Ip,
            Ipv6,
            Mac,
            Port,
            Url,
            UserAgent,
            UserName
        }
    }
}
