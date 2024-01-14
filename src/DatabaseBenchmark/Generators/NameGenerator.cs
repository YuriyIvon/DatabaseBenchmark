using Bogus;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using GeneratorKind = DatabaseBenchmark.Generators.Options.NameGeneratorOptions.GeneratorKind;

namespace DatabaseBenchmark.Generators
{
    //TODO: conceptually linked person properties
    public class NameGenerator : IGenerator
    {
        private readonly Faker _faker;
        private readonly NameGeneratorOptions _options;

        public object Current { get; private set; }

        public NameGenerator(Faker faker, NameGeneratorOptions options)
        {
            _faker = faker;
            _options = options;
        }

        public bool Next()
        {
            Current = _options.Kind switch
            {
                GeneratorKind.FirstName => _faker.Name.FirstName(),
                GeneratorKind.LastName => _faker.Name.LastName(),
                GeneratorKind.FullName => _faker.Name.FullName(),
                _ => throw new NotSupportedException()
            };

            return true;
        }
    }
}
