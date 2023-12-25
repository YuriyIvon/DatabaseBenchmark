using Bogus;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using GeneratorKind = DatabaseBenchmark.Generators.Options.VehicleGeneratorOptions.GeneratorKind;

namespace DatabaseBenchmark.Generators
{
    public class VehicleGenerator : IGenerator
    {
        private readonly Faker _faker;
        private readonly VehicleGeneratorOptions _options;

        public VehicleGenerator(Faker faker, VehicleGeneratorOptions options)
        {
            _faker = faker;
            _options = options;
        }

        public object Generate() =>
            _options.Kind switch
            {
                GeneratorKind.Manufacturer => _faker.Vehicle.Manufacturer(),
                GeneratorKind.Model => _faker.Vehicle.Model(),
                GeneratorKind.Vin => _faker.Vehicle.Vin(),
                GeneratorKind.Fuel => _faker.Vehicle.Fuel(),
                GeneratorKind.Type => _faker.Vehicle.Type(),
                _ => throw new InputArgumentException($"Unknown vehicle generator kind \"{_options.Kind}\"")
            };
    }
}
