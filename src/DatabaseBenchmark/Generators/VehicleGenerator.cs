using Bogus.DataSets;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using GeneratorKind = DatabaseBenchmark.Generators.Options.VehicleGeneratorOptions.GeneratorKind;

namespace DatabaseBenchmark.Generators
{
    public class VehicleGenerator : IGenerator
    {
        private readonly Vehicle _vehicleFaker = new();
        private readonly VehicleGeneratorOptions _options;

        public object Current { get; private set; }

        public bool IsBounded => false;

        public VehicleGenerator(VehicleGeneratorOptions options)
        {
            _options = options;
        }

        public bool Next()
        {
            Current = _options.Kind switch
            {
                GeneratorKind.Manufacturer => _vehicleFaker.Manufacturer(),
                GeneratorKind.Model => _vehicleFaker.Model(),
                GeneratorKind.Vin => _vehicleFaker.Vin(),
                GeneratorKind.Fuel => _vehicleFaker.Fuel(),
                GeneratorKind.Type => _vehicleFaker.Type(),
                _ => throw new InputArgumentException($"Unknown vehicle generator kind \"{_options.Kind}\"")
            };

            return true;
        }
    }
}
