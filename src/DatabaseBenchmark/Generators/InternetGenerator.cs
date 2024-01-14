using Bogus;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using GeneratorKind = DatabaseBenchmark.Generators.Options.InternetGeneratorOptions.GeneratorKind;

namespace DatabaseBenchmark.Generators
{
    public class InternetGenerator : IGenerator
    {
        private readonly Faker _faker;
        private readonly InternetGeneratorOptions _options;

        public object Current { get; private set; }

        public InternetGenerator(Faker faker, InternetGeneratorOptions options)
        {
            _faker = faker;
            _options = options;
        }

        public bool Next()
        {
            Current = _options.Kind switch
            {
                GeneratorKind.DomainName => _faker.Internet.DomainName(),
                GeneratorKind.Email => _faker.Internet.Email(),
                GeneratorKind.Ip => _faker.Internet.Ip(),
                GeneratorKind.Ipv6 => _faker.Internet.Ipv6(),
                GeneratorKind.Mac => _faker.Internet.Mac(),
                GeneratorKind.Port => _faker.Internet.Port(),
                GeneratorKind.Url => _faker.Internet.Url(),
                GeneratorKind.UserAgent => _faker.Internet.UserAgent(),
                GeneratorKind.UserName => _faker.Internet.UserName(),
                _ => throw new InputArgumentException($"Unknown internet generator kind \"{_options.Kind}\"")
            };

            return true;
        }
    }
}
