using Bogus;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using GeneratorKind = DatabaseBenchmark.Generators.Options.AddressGeneratorOptions.GeneratorKind;

namespace DatabaseBenchmark.Generators
{
    public class AddressGenerator : IGenerator
    {
        private readonly Faker _faker;
        private readonly AddressGeneratorOptions _options;

        public AddressGenerator(Faker faker, AddressGeneratorOptions options)
        {
            _faker = faker;
            _options = options;
        }

        public object Generate() =>
            _options.Kind switch
            {
                GeneratorKind.BuildingNumber => _faker.Address.BuildingNumber(),
                GeneratorKind.City => _faker.Address.City(),
                GeneratorKind.Country => _faker.Address.Country(),
                GeneratorKind.CountryCode => _faker.Address.CountryCode(),
                GeneratorKind.County => _faker.Address.County(),
                GeneratorKind.FullAddress => _faker.Address.FullAddress(),
                GeneratorKind.Latitude => _faker.Address.Latitude(),
                GeneratorKind.Longitude => _faker.Address.Longitude(),
                GeneratorKind.SecondaryAddress => _faker.Address.SecondaryAddress(),
                GeneratorKind.State => _faker.Address.State(),
                GeneratorKind.StreetAddress => _faker.Address.StreetAddress(),
                GeneratorKind.StreetName => _faker.Address.StreetName(),
                GeneratorKind.ZipCode => _faker.Address.ZipCode(),
                _ => throw new InputArgumentException($"Unknown address generator kind \"{_options.Kind}\"")
            };
    }
}
