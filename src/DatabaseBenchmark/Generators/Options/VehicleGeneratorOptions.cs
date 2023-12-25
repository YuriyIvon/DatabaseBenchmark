using DatabaseBenchmark.Generators.Interfaces;
using System.Text.Json.Serialization;

namespace DatabaseBenchmark.Generators.Options
{
    public class VehicleGeneratorOptions : IGeneratorOptions
    {
        public GeneratorType Type => GeneratorType.Vehicle;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GeneratorKind Kind { get; set; }

        public enum GeneratorKind
        {
            Manufacturer,
            Model,
            Vin,
            Fuel,
            Type
        }
    }
}
