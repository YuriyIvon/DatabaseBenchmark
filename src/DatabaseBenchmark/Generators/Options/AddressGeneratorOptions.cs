using DatabaseBenchmark.Generators.Interfaces;
using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Generators.Options
{
    public class AddressGeneratorOptions : IGeneratorOptions
    {
        public GeneratorType Type => GeneratorType.Address;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GeneratorKind Kind { get; set; } = GeneratorKind.Country;

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
