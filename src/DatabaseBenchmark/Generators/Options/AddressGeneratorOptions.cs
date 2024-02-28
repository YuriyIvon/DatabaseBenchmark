using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Generators.Options
{
    public class AddressGeneratorOptions : GeneratorOptionsBase
    {
        public override GeneratorType Type => GeneratorType.Address;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GeneratorKind Kind { get; set; } = GeneratorKind.Country;

        public string Locale { get; set; }

        public enum GeneratorKind
        {
            BuildingNumber,
            City,
            Country,
            CountryCode,
            County,
            FullAddress,
            Latitude,
            Longitude,
            SecondaryAddress,
            State,
            StreetAddress,
            StreetName,
            ZipCode
        }
    }
}
