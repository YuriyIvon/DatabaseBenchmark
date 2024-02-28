using Bogus.DataSets;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using GeneratorKind = DatabaseBenchmark.Generators.Options.AddressGeneratorOptions.GeneratorKind;

namespace DatabaseBenchmark.Generators
{
    public class AddressGenerator : IGenerator
    {
        private readonly Address _addressFaker;
        private readonly AddressGeneratorOptions _options;

        public object Current { get; private set; }

        public AddressGenerator(AddressGeneratorOptions options)
        {
            _options = options;
            _addressFaker = string.IsNullOrEmpty(options.Locale) ? new Address() : new Address(locale: options.Locale);
        }

        public bool Next()
        {
            Current = _options.Kind switch
            {
                GeneratorKind.BuildingNumber => _addressFaker.BuildingNumber(),
                GeneratorKind.City => _addressFaker.City(),
                GeneratorKind.Country => _addressFaker.Country(),
                GeneratorKind.CountryCode => _addressFaker.CountryCode(),
                GeneratorKind.County => _addressFaker.County(),
                GeneratorKind.FullAddress => _addressFaker.FullAddress(),
                GeneratorKind.Latitude => _addressFaker.Latitude(),
                GeneratorKind.Longitude => _addressFaker.Longitude(),
                GeneratorKind.SecondaryAddress => _addressFaker.SecondaryAddress(),
                GeneratorKind.State => _addressFaker.State(),
                GeneratorKind.StreetAddress => _addressFaker.StreetAddress(),
                GeneratorKind.StreetName => _addressFaker.StreetName(),
                GeneratorKind.ZipCode => _addressFaker.ZipCode(),
                _ => throw new InputArgumentException($"Unknown address generator kind \"{_options.Kind}\"")
            };

            return true;
        }
    }
}
