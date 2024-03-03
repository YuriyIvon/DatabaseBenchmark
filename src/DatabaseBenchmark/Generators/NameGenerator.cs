using Bogus.DataSets;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using GeneratorKind = DatabaseBenchmark.Generators.Options.NameGeneratorOptions.GeneratorKind;

namespace DatabaseBenchmark.Generators
{
    //TODO: conceptually linked person properties
    public class NameGenerator : IGenerator
    {
        private readonly Name _nameFaker;
        private readonly NameGeneratorOptions _options;

        public object Current { get; private set; }

        public bool IsBounded => false;

        public NameGenerator(NameGeneratorOptions options)
        {
            _options = options;
            _nameFaker = string.IsNullOrEmpty(options.Locale) ? new Name() : new Name(locale: options.Locale);
        }

        public bool Next()
        {
            Current = _options.Kind switch
            {
                GeneratorKind.FirstName => _nameFaker.FirstName(),
                GeneratorKind.LastName => _nameFaker.LastName(),
                GeneratorKind.FullName => _nameFaker.FullName(),
                _ => throw new NotSupportedException()
            };

            return true;
        }
    }
}
