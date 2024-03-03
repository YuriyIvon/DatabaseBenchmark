using Bogus.DataSets;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using GeneratorKind = DatabaseBenchmark.Generators.Options.InternetGeneratorOptions.GeneratorKind;

namespace DatabaseBenchmark.Generators
{
    public class InternetGenerator : IGenerator
    {
        private readonly Internet _internetFaker;
        private readonly InternetGeneratorOptions _options;

        public object Current { get; private set; }

        public bool IsBounded => false;

        public InternetGenerator(InternetGeneratorOptions options)
        {
            _options = options;
            _internetFaker = string.IsNullOrEmpty(options.Locale) ? new Internet() : new Internet(locale: options.Locale);
        }

        public bool Next()
        {
            Current = _options.Kind switch
            {
                GeneratorKind.DomainName => _internetFaker.DomainName(),
                GeneratorKind.Email => _internetFaker.Email(),
                GeneratorKind.Ip => _internetFaker.Ip(),
                GeneratorKind.Ipv6 => _internetFaker.Ipv6(),
                GeneratorKind.Mac => _internetFaker.Mac(),
                GeneratorKind.Port => _internetFaker.Port(),
                GeneratorKind.Url => _internetFaker.Url(),
                GeneratorKind.UserAgent => _internetFaker.UserAgent(),
                GeneratorKind.UserName => _internetFaker.UserName(),
                _ => throw new InputArgumentException($"Unknown internet generator kind \"{_options.Kind}\"")
            };

            return true;
        }
    }
}
