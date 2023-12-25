using Bogus;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using GeneratorKind = DatabaseBenchmark.Generators.Options.CompanyGeneratorOptions.GeneratorKind;

namespace DatabaseBenchmark.Generators
{
    public class CompanyGenerator : IGenerator
    {
        private readonly Faker _faker;
        private readonly CompanyGeneratorOptions _options;

        public CompanyGenerator(Faker faker, CompanyGeneratorOptions options)
        {
            _faker = faker;
            _options = options;
        }

        public object Generate() =>
            _options.Kind switch
            {
                GeneratorKind.CompanySuffix => _faker.Company.CompanySuffix(),
                GeneratorKind.CompanyName => _faker.Company.CompanyName(),
                _ => throw new InputArgumentException($"Unknown company generator kind \"{_options.Kind}\"")
            };
    }
}
