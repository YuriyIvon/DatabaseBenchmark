using Bogus.DataSets;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using GeneratorKind = DatabaseBenchmark.Generators.Options.CompanyGeneratorOptions.GeneratorKind;

namespace DatabaseBenchmark.Generators
{
    public class CompanyGenerator : IGenerator
    {
        private readonly Company _companyFaker;
        private readonly CompanyGeneratorOptions _options;

        public object Current { get; private set; }

        public bool IsBounded => false;

        public CompanyGenerator(CompanyGeneratorOptions options)
        {
            _options = options;
            _companyFaker = string.IsNullOrEmpty(options.Locale) ? new Company() : new Company(locale: _options.Locale);
        }

        public bool Next()
        {
            Current = _options.Kind switch
            {
                GeneratorKind.CompanySuffix => _companyFaker.CompanySuffix(),
                GeneratorKind.CompanyName => _companyFaker.CompanyName(),
                _ => throw new InputArgumentException($"Unknown company generator kind \"{_options.Kind}\"")
            };

            return true;
        }
    }
}
